/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
using API.Services;
using API.Tools;

namespace IntegrationTests;

public class LevelTests : AbstractAsyncLifetime
{
    async Task<IdentityResult?> CreateUser(IServiceScope scope, int userId)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var user = new User
        {
            UserName = "user1",
            Email = "user1@test.com",
            DisplayName = "user1",
            FirstName = "Test",
            LastName = "User",
            Id = userId
        };

        var result = await userManager.CreateAsync(user, "Password123!");
        return result;
    }

    private Item CreateItem(AppDbContext db, int id, int scopeId = 1, Item.ItemSource source = Item.ItemSource.LLM, Item.Language language = Item.Language.Dutch)
    {
        var scope = db.Scopes.Local.FirstOrDefault(t => t.Id == scopeId);

        if (scope == null)
        {
            scope = db.Scopes.FirstOrDefault(t => t.Id == scopeId)
                     ?? new Scope { Id = scopeId, Name = $"Topic{scopeId}" };

            if (db.Entry(scope).State == EntityState.Detached)
            {
                db.Scopes.Add(scope);
                db.SaveChanges();
            }
        }

        var item = new Item
        {
            Id = id,
            Active = true,
            Type = Item.ItemType.MultipleChoice,
            Source = source,
            Lang = language,
            QuestionText = $"Question {id}",
            ResponseType = "conceptual",
            Level = "easy",
            AppearanceCount = 0
        };

        item.Answers.Add(new ItemAnswer
        {
            AnswerText = "Answer A",
            Correct = true,
            AnswerIdentifier = "A"
        });

        item.Scopes.Add(scope);

        db.Items.Add(item);
        db.SaveChanges();

        db.ItemUrns.Add(new ItemUrn { ItemId = id, GreenBalls = 0, RedBalls = 0, Item = item });

        return item;
    }


    // Using Fact instead of Theory because Theory is executed in parallel and causes parallel database connection issues. 
    // When using theory, sometimes the database can be closed whilst some tests are still running causing an error to be thrown. 
    // This would make the tests flaky. Fact is executed sequentially, and avoid this problem
    [Fact]
    public Task Submit_level_admin_all_correct() => Get_and_submit_level(
        userId: 1, role: "Admin", expected: 200,
        currentStreak: 0, highestStreak: 0, expCurStreak: 1, expHighStreak: 1,
        correctAnswers: [true, true, true, true, true],
        expectMastery: true);

    [Fact]
    public Task Submit_level_user_partial_correct() => Get_and_submit_level(
        userId: 2, role: "User", expected: 200,
        currentStreak: 5, highestStreak: 6, expCurStreak: 6, expHighStreak: 6,
        correctAnswers: [true, true, false, true, false],
        expectMastery: false);

    [Fact]
    public Task Submit_level_user_mostly_correct() => Get_and_submit_level(
        userId: 3, role: "User", expected: 200,
        currentStreak: 4, highestStreak: 7, expCurStreak: 5, expHighStreak: 7,
        correctAnswers: [true, false, true, true, true],
        expectMastery: true);

    [Fact]
    public Task Submit_level_user_all_wrong() => Get_and_submit_level(
        userId: 4, role: "User", expected: 200,
        currentStreak: 4, highestStreak: 7, expCurStreak: 5, expHighStreak: 7,
        correctAnswers: [false, false, false, false, false],
        expectMastery: false);

    private async Task Get_and_submit_level(
        int userId,
        string role,
        int expected,
        int currentStreak,
        int highestStreak,
        int expCurStreak,
        int expHighStreak,
        bool[] correctAnswers,
        bool expectMastery)
    {
        int topicId = 158;

        using var scope = _factory.Services.CreateScope();
        var db = _factory.CreateDbContext();
        await db.Database.EnsureCreatedAsync();

        var client = _factory.CreateClientAs(
            userId,
            $"{userId}@test.com",
            role);

        // Create user with all meta data
        var result = await CreateUser(scope, userId);

        db.Scopes.Add(new Scope { Id = topicId, Name = "Test" });
        db.SaveChanges();
        var topicService = scope.ServiceProvider.GetRequiredService<ITopicService>();
        await topicService.DefaultUserProficiencies(userId);
        var user = await db.Users.FirstAsync(u => u.Id == userId);

        var timeTool = scope.ServiceProvider.GetRequiredService<TimeTool>();
        db.UserStreaks.Add(new UserStreak { UserId = userId, LastDayCompleted = timeTool.Today().AddDays(-1), CurrentStreak = currentStreak, HighestStreak = highestStreak });

        // Item creation
        List<Item> items = new();
        for (int i = 1; i <= 4; i++)
        {
            var item = CreateItem(db, i, topicId, Item.ItemSource.Databank);
            items.Add(item);
        }
        var AiItem = CreateItem(db, 5, topicId, Item.ItemSource.LLM);
        items.Add(AiItem);

        // Change levelsize in settings
        db.Settings.Add(new Setting { ProfileName = "test", LastActive = DateTime.UtcNow.AddHours(1), AiFactor = 0.2, LevelSize = 5, Id = 3 });
        db.SaveChanges();


        // Start a level
        var content = new StringContent("158", Encoding.UTF8, "application/json");
        var res = await client.PostAsync("/api/level/randomlevel", content);

        var levelResult = await db.LevelResults.FirstAsync();

        // Finish a level
        var payload = new
        {
            levelResultId = levelResult.Id,
            answers = items.Select((item, index) => new
            {
                itemId = item.Id,
                levelId = levelResult.Id,
                correct = correctAnswers[index],
                completionStatus = "completed",
                answer = (string?)null,
                answerIdentifier = "A"
            })
        };

        var levelResultContent = JsonContent.Create(payload);
        var response = await client.PostAsync("/api/level/submitlevel", levelResultContent);

        db.ChangeTracker.Clear();

        // Assert
        Assert.Equal(expected, (int)response.StatusCode);

        var userTopicProgress = await db.UserScopeProgress.FirstAsync(tp => tp.UserId == userId && tp.ScopeId == topicId);
        Assert.Equal(expectMastery, userTopicProgress.Mastered);

        var userStreak = await db.UserStreaks.FirstAsync(us => us.UserId == userId);
        Assert.Equal(expCurStreak, userStreak.CurrentStreak);
        Assert.Equal(expHighStreak, userStreak.HighestStreak);
    }
}
