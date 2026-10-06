/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA;
using AA;
using AA.PickItemsAlgorithm;
using AA.PickItemsAlgorithm;
using AA.UrningsAlgorithm;
using AA.UrningsAlgorithm;
using AA.UserMasteryAlgorithm;
using AA.UserMasteryAlgorithm;
using AA.UserProficiencyAlgorithm;
using AA.UserProficiencyAlgorithm;
using API.Controllers.DTOs;
using API.Controllers.DTOs;
using API.Handlers;
using API.Handlers.GameEventHandlers;
using API.Services;
using API.Tools;
using APITests.Tools;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using Moq;
using System.Runtime.InteropServices.JavaScript;

namespace APITests.Services;

public class LevelServiceTests
{
    private readonly AppDbContext db;
    private readonly Mock<IItemPoolService> itemPoolService;
    private readonly Mock<ITopicService> topicService;
    private readonly Mock<IItemService> itemService;
    private readonly Mock<IUserCurrencyService> userCurrencyService;
    private readonly Mock<IAlgorithmRegistry<IUserProficiencyAlgorithm>> upa;
    private readonly Mock<IAlgorithmRegistry<IUserMasteryAlgorithm>> uma;
    private readonly Mock<IAlgorithmRegistry<IUrningsAlgorithm>> ua;
    private readonly Mock<IAlgorithmRegistry<IPickItemsAlgorithm>> pi;
    private readonly LevelHandler levelHandler;
    private readonly LevelService service;
    private readonly Mock<IUrningsAlgorithm> _mockUa;

    public LevelServiceTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        db = new AppDbContext(options);
        db.Database.EnsureCreated();

        itemPoolService = new Mock<IItemPoolService>();
        topicService = new Mock<ITopicService>();
        itemService = new Mock<IItemService>();
        userCurrencyService = new Mock<IUserCurrencyService>();
        levelHandler = new LevelHandler();
        upa = new Mock<IAlgorithmRegistry<IUserProficiencyAlgorithm>>();
        pi = new Mock<IAlgorithmRegistry<IPickItemsAlgorithm>>();

        // Arrange the  UA mock
        _mockUa = new Mock<IUrningsAlgorithm>();
        _mockUa
            .Setup(a => a.UpdateUrnings(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
        ua = new Mock<IAlgorithmRegistry<IUrningsAlgorithm>>();

        uma = new Mock<IAlgorithmRegistry<IUserMasteryAlgorithm>>();

        // Setup TimeTool for testing timings
        DateTime fakeNow = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        TimeTool timeTool = new TimeTool(timeProvider: new FakeTimeProvider(fakeNow));

        service = new LevelService(db, itemPoolService.Object, topicService.Object, itemService.Object, levelHandler, userCurrencyService.Object, upa.Object, uma.Object, ua.Object, pi.Object, Mock.Of<IGameEventManager>(), Mock.Of<ILogger<LevelService>>(), timeTool);
    }

    private User CreateUser(int id = 1)
    {
        return new User
        {
            Id = id,
            DisplayName = $"user{id}",
            FirstName = "Test",
            LastName = "User",
            Email = $"user{id}@example.com",
            PasswordHash = "x"
        };
    }

    private Item CreateItem(int id, int topicId = 1, Item.ItemSource source = Item.ItemSource.LLM, Item.Language language = Item.Language.Dutch)
    {
        var topic = db.Scopes.Local.FirstOrDefault(t => t.Id == topicId);

        if (topic == null)
        {
            topic = db.Scopes.FirstOrDefault(t => t.Id == topicId)
                     ?? new Scope { Id = topicId, Name = $"Topic{topicId}" };

            if (db.Entry(topic).State == EntityState.Detached)
            {
                db.Scopes.Add(topic);
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

        item.Scopes.Add(topic); 

        return item;
    }

    [Fact]
    public async Task GetRandomLevel_WhenUserMissing_Throws()
    {
        await Assert.ThrowsAsync<Exception>(() => service.GetRandomLevel(999, 5, Item.Language.Dutch));
    }

    [Fact(Skip = "Temporarily disabled this test because triggering itempool is disabled in GetRandomLevel because of fail error in console and not being used.")]
    public async Task GetRandomLevel_ReturnsXmlAndLevel_AndTriggersItemPool()
    {
        var user = CreateUser(1);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var item = CreateItem(1, 10, Item.ItemSource.Databank);
        db.Items.Add(item);
        await db.SaveChangesAsync();

        topicService.Setup(s => s.GetUnlockedTopics(1))
            .ReturnsAsync(new List<ScopeDTO> { new ScopeDTO { ScopeId = 10, ScopeName = "Topic10"} });

        itemService.Setup(s => s.NotRecentlySeenItems(1))
            .Returns(db.Items.Include(i => i.Scopes));

        var (xml, itemIds, level) = await service.GetRandomLevel(1, 0, Item.Language.Dutch);

        Assert.NotNull(xml);
        Assert.NotEmpty(xml);
        Assert.NotNull(level);
        Assert.Single(level.UserAnswer);
        Assert.Single(itemIds);

        itemPoolService.Verify(s => s.GenerateItemsTillThreshold(1), Times.Once);
    }

    [Fact]
    public async Task GenerateRandom_WhenNotEnoughItems_Throws()
    {
        var user = CreateUser(1);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        pi.Setup(f => f.Get("V1PickItems")).Returns(new V1PickItems(db));

        topicService.Setup(s => s.GetUnlockedTopics(1))
            .ReturnsAsync(new List<ScopeDTO>());

        itemService.Setup(s => s.NotRecentlySeenItems(1))
            .Returns(db.Items.Where(i => false)); // empty EF IQueryable

        await Assert.ThrowsAsync<Exception>(() =>
            service.GenerateRandom(user, 0, Item.Language.Dutch));
    }

    [Fact]
    public async Task GenerateRandom_FiltersByUserTopics_AndRequestedTopics()
    {
        var user = CreateUser(1);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var item1 = CreateItem(1, 10, Item.ItemSource.Databank);
        var item2 = CreateItem(2, 20, Item.ItemSource.Databank);
        var item3 = CreateItem(3, 30, Item.ItemSource.Databank);

        db.Items.AddRange(item1, item2, item3);
        db.Settings.Add(new Setting { ProfileName = "whatever", LevelSize = 1, LastActive = DateTime.Now });
        await db.SaveChangesAsync();

        pi.Setup(f => f.Get("V1PickItems")).Returns(new V1PickItems(db));

        //itemService.Setup(s => s.NotRecentlySeenItems(1))
        //    .Returns(db.Items.Include(i => i.Topics));

        var result = await service.GenerateRandom(user, 20, Item.Language.Dutch);

        Assert.Single(result.UserAnswer);
        Assert.Equal(2, result.UserAnswer.First().Item.Id);
    }

    [Fact]
    public async Task GenerateRandom_FiltersByLanguage()
    {
        var user = CreateUser(1);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var item1 = CreateItem(1, 20, Item.ItemSource.Databank, Item.Language.Dutch);
        var item2 = CreateItem(2, 20, Item.ItemSource.Databank, Item.Language.English);

        db.Items.AddRange(item1, item2);
        db.Settings.Add(new Setting { ProfileName = "whatever", LevelSize = 1, LastActive = DateTime.Now });
        await db.SaveChangesAsync();

        pi.Setup(f => f.Get("V1PickItems")).Returns(new V1PickItems(db));

        //itemService.Setup(s => s.NotRecentlySeenItems(1))
        //    .Returns(db.Items.Include(i => i.Topics));

        var result = await service.GenerateRandom(user, 20, Item.Language.Dutch);

        Assert.Single(result.UserAnswer);
        Assert.Equal(1, result.UserAnswer.First().Item.Id);

        result = await service.GenerateRandom(user, 20, Item.Language.English);

        Assert.Single(result.UserAnswer);
        Assert.Equal(2, result.UserAnswer.First().Item.Id);
    }


    [Fact]
    public async Task UpdateUserAnswer_WhenItemMissing_Throws()
    {
        var req = new UserAnswerRequest { ItemId = 999, LevelId = 1 };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateUserAnswer(req));
    }

    [Fact]
    public async Task UpdateUserAnswer_WhenUserAnswerMissing_Throws()
    {
        var item = CreateItem(1);
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var req = new UserAnswerRequest { ItemId = 1, LevelId = 1 };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateUserAnswer(req));
    }

    [Fact]
    public async Task UpdateUserAnswer_NormalizesEmptyAnswer()
    {
        var user = CreateUser(1);
        db.Users.Add(user);

        var item = CreateItem(1, 10);
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var level = new LevelResult { UserId = 1, User = user };
        db.LevelResults.Add(level);
        await db.SaveChangesAsync();

        var uAnswer = new UserAnswer
        {
            ItemId = item.Id,
            Item = item,
            LevelResultId = level.Id,
            LevelResult = level
        };
        db.UserAnswers.Add(uAnswer);
        await db.SaveChangesAsync();

        var mockAlgorithm = new Mock<IUserProficiencyAlgorithm>();
        mockAlgorithm.Setup(a => a.UpdateProficiency(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        upa.Setup(f => f.Get("Simple UPA")).Returns(mockAlgorithm.Object);
        ua.Setup(r => r.Get("Basic Urnings Algorithm")).Returns(_mockUa.Object);

        var req = new UserAnswerRequest
        {
            ItemId = item.Id,
            LevelId = level.Id,
            Answer = " ",
            AnswerIdentifier = "A",
            Correct = true,
            CompletionStatus = "done"
        };

        await service.UpdateUserAnswer(req);

        var updated = await db.UserAnswers.FirstAsync();
        Assert.Equal("Answer A", updated.Answer);
        Assert.Equal("A", updated.AnswerIdentifier);
        Assert.True(updated.Correct);
    }

    [Fact]
    public async Task SubmitLevel_WhenLevelMissing_Throws()
    {
        var logger = new Mock<ILogger>();
        var req = new LevelAnswerRequest { LevelResultId = 999 };

        await Assert.ThrowsAsync<Exception>(() =>
            service.SubmitLevel(req, logger.Object));
    }

    [Fact]
    public async Task SubmitLevel_UpdatesAllAnswers()
    {
        var user = CreateUser(1);
        db.Users.Add(user);

        var item = CreateItem(1, 10);
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var topic = new Scope { Id = 1, Name = "john pork" };
        db.Scopes.Add(topic);
        await db.SaveChangesAsync();

        var up = new UserScopeProgress { UserId = 1, ScopeId = 1, Mastered = false };
        db.UserScopeProgress.Add(up);

        var streak = new UserStreak { UserId = 1, LastDayCompleted = DateTime.MinValue, CurrentStreak = 0, HighestStreak = 0 };
        db.UserStreaks.Add(streak);

        var userAnswer = new UserAnswer
        {
            ItemId = item.Id,
            Item = item,
            LevelResultId = 1,
        };
        var level = new LevelResult { Id = 1, UserId = 1, TopicId = 1, User = user, UserAnswer = [userAnswer, userAnswer, userAnswer] };

        db.LevelResults.Add(level);
        db.UserAnswers.Add(userAnswer);
        await db.SaveChangesAsync();

        var mockAlgorithm = new Mock<IUserProficiencyAlgorithm>();
        mockAlgorithm.Setup(a => a.UpdateProficiency(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        upa.Setup(f => f.Get("Simple UPA")).Returns(mockAlgorithm.Object);
        ua.Setup(r => r.Get("Basic Urnings Algorithm")).Returns(_mockUa.Object);
        uma.Setup(f => f.Get("Simple UMA")).Returns(new SimpleUMA(db));

        var logger = new Mock<ILogger>();

        var req = new LevelAnswerRequest
        {
            LevelResultId = level.Id,
            Answers = new()
            {
                new UserAnswerRequest
                {
                    ItemId = item.Id,
                    LevelId = level.Id,
                    Answer = "Final",
                    AnswerIdentifier = "A",
                    Correct = true,
                    CompletionStatus = "done"
                }
            }
        };

        var result = await service.SubmitLevel(req, logger.Object);

        Assert.Equal(level.Id, result.Id);

        var updated = await db.UserAnswers.FirstAsync();
        Assert.Equal("Final", updated.Answer);
        Assert.True(updated.Correct);
    }

    [Fact]
    public async Task SubmitLevel_WhenUpdateFails_ContinuesSavingAnswers()
    {
        var user = CreateUser(1);
        db.Users.Add(user);
        Scope scope = new Scope
        {
            Id = 2,
            Name = "LOOOL"
        };
        db.Scopes.Add(scope);
        LevelResult level = new LevelResult
        {
            Id = 5,
            User = user,
            CreatedAt = DateTime.UtcNow,
            TopicId = 2
        };
        db.LevelResults.Add(level);
        Item item = CreateItem(5);
        db.Items.Add(item);
        UserAnswer answer = new UserAnswer
        {
            Id = 8,
            LevelResultId = 5,
            Item = item,
            Correct = false
        }; 
        db.UserAnswers.Add(answer);
        await db.SaveChangesAsync();
        
        var request1 = new UserAnswerRequest
        {
            Answer = "lol",
            Correct = true,
            ItemId = 9999999, // Doesn't exist, throws exception
            LevelId = level.Id
        };
        var request2 = new UserAnswerRequest
        {
            Answer = "lol",
            Correct = true,
            ItemId = item.Id,
            LevelId = level.Id
        };
        var levelReq = new LevelAnswerRequest
        {
            LevelResultId = level.Id,
            Answers = [request1, request2]
        };
        
        var streak = new UserStreak { UserId = 1, LastDayCompleted = DateTime.MinValue, CurrentStreak = 0, HighestStreak = 0 };
        db.UserStreaks.Add(streak);
        
        var mockAlgorithm = new Mock<IUserProficiencyAlgorithm>();
        mockAlgorithm.Setup(a => a.UpdateProficiency(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        upa.Setup(f => f.Get("Simple UPA")).Returns(mockAlgorithm.Object);
        ua.Setup(r => r.Get("Basic Urnings Algorithm")).Returns(_mockUa.Object);
        uma.Setup(f => f.Get("Simple UMA")).Returns(new SimpleUMA(db));

        await service.SubmitLevel(levelReq, Mock.Of<ILogger>());

        Assert.Equal(true, (await db.UserAnswers.FindAsync(answer.Id))?.Correct);
    }

    private async Task<LevelAnswerRequest> SetupStreakTest(UserStreak streak)
    {
        var user = CreateUser(1);
        db.Users.Add(user);
        db.UserStreaks.Add(streak);

        var item = CreateItem(1, 10);
        db.Items.Add(item);
        db.Scopes.Add(new Scope {
            Id = 1,
            Name = "john pork"
        });
        db.UserScopeProgress.Add(new UserScopeProgress {
            UserId = 1,
            ScopeId = 1,
            Mastered = false
        });
        await db.SaveChangesAsync();

        var userAnswer = new UserAnswer {
            ItemId = item.Id,
            Item = item,
            LevelResultId = 1
        };
        db.LevelResults.Add(new LevelResult {
            Id = 1,
            UserId = 1,
            TopicId = 1,
            User = user,
            UserAnswer = [userAnswer]
        });
        db.UserAnswers.Add(userAnswer);
        await db.SaveChangesAsync();

        var mockUpa = new Mock<IUserProficiencyAlgorithm>();
        mockUpa.Setup(a => a.UpdateProficiency(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        upa.Setup(f => f.Get("Simple UPA")).Returns(mockUpa.Object);
        ua.Setup(r => r.Get("Basic Urnings Algorithm")).Returns(_mockUa.Object);
        uma.Setup(f => f.Get("Simple UMA")).Returns(new SimpleUMA(db));
        itemPoolService.Setup(s => s.QueueNewItemGeneration(It.IsAny<int>())).Returns(Task.CompletedTask);

        return new LevelAnswerRequest
        {
            LevelResultId = 1,
            Answers = new()
            {
                new UserAnswerRequest
                {
                    ItemId = item.Id,
                    LevelId = 1,
                    Answer = "Final",
                    AnswerIdentifier = "A",
                    Correct = true,
                    CompletionStatus = "done"
                }
            }
        };
    }

    private readonly static DateTime fakeNow = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc);

    // Test cases for streak updates: starting from MinValue, updating the day after, and updating without reaching a new highest streak.
    private static UserStreak updateFromMinValue = new UserStreak { UserId = 1, LastDayCompleted = DateTime.MinValue, CurrentStreak = 0, HighestStreak = 0 };
    private static UserStreak updateDayAfter = new UserStreak { UserId = 1, LastDayCompleted = fakeNow.AddDays(-1), CurrentStreak = 1, HighestStreak = 1 };
    private static UserStreak updateWithoutHighest = new UserStreak { UserId = 1, LastDayCompleted = fakeNow.AddDays(-1), CurrentStreak = 1, HighestStreak = 3 };

    // Using MemberData to run the same test with different initial streak states and expected outcomes.
    public static IEnumerable<object[]> StreakTestCases() =>
    [
        [updateFromMinValue, 1, 1],
        [updateDayAfter, 2, 2],
        [updateWithoutHighest, 2, 3],
    ];

    // This is the actual test method that will be run for each of the test cases defined above. It checks if the streak is updated correctly based on the initial state.
    [Theory]
    [MemberData(nameof(StreakTestCases))]
    public async Task UpdateStreak(UserStreak initialStreak, int expectedCurrent, int expectedHighest)
    {
        var req = await SetupStreakTest(initialStreak);
        await service.SubmitLevel(req, new Mock<ILogger>().Object);

        var updatedStreak = await db.UserStreaks.FindAsync(1);
        Assert.Equal(expectedCurrent, updatedStreak.CurrentStreak);
        Assert.Equal(expectedHighest, updatedStreak.HighestStreak);
    }
}
