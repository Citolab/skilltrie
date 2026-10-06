/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.NewItemsAlgorithm;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Reflection;
using Xunit;

namespace AATests.NewItemsAlgorithm;

public class SimpleNIATest
{

    private readonly AppDbContext db;
    private readonly SimpleNIA _algorithm;

    public SimpleNIATest()
    {
        db = SqliteInMemoryContextFactory.Create();
        _algorithm = new SimpleNIA(db);
    }

    private Item CreateItem(int id = 0, string text = "Hello world")
    {
        return new Item
        {
            Id = id,
            Active = true,
            Type = Item.ItemType.MultipleChoice,
            Source = Item.ItemSource.LLM,
            Lang = Item.Language.English,
            QuestionText = text,
            ResponseType = "conceptual",
            AppearanceCount = 0,
            Level = "easy",
            Answers = new List<ItemAnswer>
            {
                new ItemAnswer { AnswerText = "A", Correct = true, AnswerIdentifier = "A" }
            }
        };
    }

    [Fact]
    public async Task NewItemBatch_ThrowsWhenUserNotFound()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => _algorithm.NewItemBatch(1, 99));
        Assert.Equal("User not found", ex.Message);
    }

    [Fact]
    public async Task NewItemBatch_ThrowsWhenTopicNotFound()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<Exception>(() => _algorithm.NewItemBatch(0, 1));
        Assert.Equal("Topic not found", ex.Message);
    }

    [Fact]
    public async Task NewItemBatch_ThrowsWhenActiveSettingNotFound()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "Test" });
        db.Settings.RemoveRange(db.Settings);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<Exception>(() => _algorithm.NewItemBatch(1, 1));
        Assert.Equal("No active setting found", ex.Message);
    }

    [Fact]
    public async Task NewItemBatch_SufficientItemsAvailable()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "Test" });
        db.Settings.Add(new Setting { Id = 20, LevelSize = 5, ProfileName = "Temp", TimeBeforeRedo = new TimeSpan(1, 0, 0, 0), LastActive = DateTime.UtcNow + new TimeSpan(0, 3, 0) });
        for (int i = 1; i <= 10; i++)
        {
            Item item = CreateItem(i);
            db.Items.Add(item);
            db.ScopeItems.Add(new ScopeItem { ItemId = i, ScopeId = 1 });
        }
        db.LevelResults.Add(new LevelResult { Id = 1, UserId = 1, CreatedAt = DateTime.UtcNow, TopicId = 1 });
        db.UserAnswers.Add(new UserAnswer { Id = 1, ItemId = 1, LevelResultId = 1, CompletionStatus = "completed", AnswerDate = DateTime.UtcNow, Correct = true });
        await db.SaveChangesAsync();

        var result = await _algorithm.NewItemBatch(1, 1);
        Assert.Equal(new NewItemsRecord { TopicId = 1, UserId = 1 }, result);

    }

    [Fact]
    public async Task NewItemBatch_NewItemsRequired()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "Test" });
        db.Settings.Add(new Setting { Id = 20, LevelSize = 5, ProfileName = "Temp", TimeBeforeRedo = new TimeSpan(1, 0, 0, 0), LastActive = DateTime.UtcNow + new TimeSpan(0, 3, 0) });
        for (int i = 1; i <= 5; i++)
        {
            Item item = CreateItem(i);
            db.Items.Add(item);
            db.ScopeItems.Add(new ScopeItem { ItemId = i, ScopeId = 1 });
        }
        db.LevelResults.Add(new LevelResult { Id = 1, UserId = 1, CreatedAt = DateTime.UtcNow, TopicId = 1 });
        db.UserAnswers.Add(new UserAnswer { Id = 1, ItemId = 1, LevelResultId = 1, CompletionStatus = "completed", AnswerDate = DateTime.UtcNow, Correct = true });
        await db.SaveChangesAsync();

        var result = await _algorithm.NewItemBatch(1, 1);
        Assert.Equal(new NewItemsRecord { TopicId = 1, UserId = 1, TotalNewItems = 1, MultipleChoice = 1 }, result);
    }

}

