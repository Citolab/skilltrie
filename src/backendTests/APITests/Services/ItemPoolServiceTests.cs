/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using API.Services;
using API.Tools.AI;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using API.Handlers;
using Xunit;
using AA;
using AA.NewItemsAlgorithm;
using API.Tools.EventQueue;
using System.Threading.Channels;

namespace APITests.Services;

public class ItemPoolServiceTests
{
    private readonly AppDbContext db;
    private readonly Mock<IAIService> aiService;
    private readonly Mock<IItemService> itemService;
    private readonly Mock<ILogger<ItemPoolService>> logger;
    private readonly ItemPoolService service;

    public ItemPoolServiceTests()
    {
        // Setup in-memory SQLite
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        db = new AppDbContext(options);
        db.Database.EnsureCreated();

        // Mock dependencies
        aiService = new Mock<IAIService>();

        itemService = new Mock<IItemService>();
        logger = new Mock<ILogger<ItemPoolService>>();

        service = new ItemPoolService(db, aiService.Object, itemService.Object, Mock.Of<IAlgorithmRegistry<INewItemsAlgorithm>>(), logger.Object, Mock.Of<IEventQueue>());
    }

    private Item CreateValidItem(string answerIdentifier = "section_ABC", string questionText = "What is 2 + 2?")
    {
        return new Item
        {
            Active = true,
            Type = Item.ItemType.MultipleChoice,
            Source = Item.ItemSource.LLM,
            Lang = Item.Language.English,
            QuestionText = questionText,
            ResponseType = "conceptual",
            AppearanceCount = 0,
            Level = "easy",
            AnswerExplanation = "Basic arithmetic",
            Answers = new List<ItemAnswer>
            {
                new ItemAnswer
                {
                    AnswerText = "4",
                    Correct = true,
                    Chosen = 0,
                    AnswerIdentifier = answerIdentifier
                }
            }
        };
    }

    private Scope CreateTopic(int id, string name)
    {
        return new Scope { Id = id, Name = name };
    }

    [Fact]
    public async Task GenerateItemsTillThreshold_NoGenerationNeeded()
    {
        // Arrange
        var topic = CreateTopic(1, "Math");
        db.Scopes.Add(topic);

        var item = CreateValidItem();
        item.ScopeItems.Add(new ScopeItem { ScopeId = topic.Id});
        db.Items.Add(item);
        await db.SaveChangesAsync();

        itemService.Setup(s => s.NotRecentlySeenItems(123))
            .Returns(db.Items);

        var localService = new ItemPoolService(
            db,
            aiService.Object,
            itemService.Object,
            Mock.Of<IAlgorithmRegistry<INewItemsAlgorithm>>(),
            logger.Object,
            Mock.Of<IEventQueue>()
        );

        // Act
        await localService.GenerateItemsTillThreshold(123);

        // Assert
        aiService.Verify(
            a => a.MakeQuestions(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never
        );
    }


    [Fact]
    public async Task GenerateItemsTillThreshold_GeneratesMissingItems()
    {
        // Arrange
        var topic = CreateTopic(1, "Math");
        db.Scopes.Add(topic);

        var topicItem = CreateValidItem();
        topicItem.ScopeItems.Add(new ScopeItem { ScopeId = topic.Id });
        db.Items.Add(topicItem);

        var unrelatedItem = CreateValidItem(questionText: "Unrelated question");
        db.Items.Add(unrelatedItem);

        await db.SaveChangesAsync();

        itemService.Setup(s => s.NotRecentlySeenItems(123))
            .Returns(db.Items.Where(i => i.Id == unrelatedItem.Id));

        // Mock AI API to return a new generated item
        var generatedItem = new List<Item>() { CreateValidItem("section_NEW", "Generated Question?") };
        aiService.Setup(a => a.MakeQuestions(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(generatedItem);

        // Act
        await service.GenerateItemsTillThreshold(123);

        // Assert
        aiService.Verify(a => a.MakeQuestions(It.IsAny<int>(), It.IsAny<int>()), Times.Once);

        var savedItems = db.Items.ToList();
        Assert.Equal(3, savedItems.Count);
        Assert.Equal(2, db.ScopeItems.Count());
    }

    [Fact]
    public async Task GenNewItem_UpdatesAnswerIdentifier()
    {
        // Arrange
        var topic = CreateTopic(1, "Math");
        db.Scopes.Add(topic);
        await db.SaveChangesAsync();

        itemService.Setup(s => s.NotRecentlySeenItems(123))
            .Returns(db.Items.Where(i => false));

        var generatedItem = new List<Item>() { CreateValidItem("section_NEW", "Generated Question?") };
        aiService.Setup(a => a.MakeQuestions(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(generatedItem);

        // Act
        await service.GenerateItemsTillThreshold(123);

        var saved = db.Items.Include(i => i.Answers).Single();
        var answer = saved.Answers.Single();

        Assert.StartsWith("section_", answer.AnswerIdentifier);
        Assert.Contains($"{saved.Id}_item_1_num_RESPONSE_1_", answer.AnswerIdentifier);
    }

    [Fact]
    public async Task GenNewItem_WhenSaveFails_RollsBackAndThrows()
    {
        // Arrange: create a topic
        var topic = CreateTopic(1, "Math");
        db.Scopes.Add(topic);
        await db.SaveChangesAsync();

        // NotRecentlySeenItems returns no items → generation triggers
        itemService.Setup(s => s.NotRecentlySeenItems(123))
            .Returns(db.Items.Where(i => false));

        // AI returns an item with an INVALID FK to force SQLite to throw
        var generated = new List<Item>() { CreateValidItem("section_ABC") };

        // This TopicItem has a TopicId that does NOT exist → FK violation
        generated[0].ScopeItems.Add(new ScopeItem { ScopeId = 9999 });

        aiService.Setup(a => a.MakeQuestions(1, 1))
            .ReturnsAsync(generated);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.GenerateItemsTillThreshold(123));

        Assert.Contains("Error trying to generate items for topic 1", ex.Message);

        // Ensure rollback happened
        Assert.Empty(db.Items);
        Assert.Empty(db.ScopeItems);
    }


    [Fact]
    public async Task GenerateItemsTillThreshold_MultipleTopics()
    {
        var d1 = CreateTopic(1, "Math");
        var d2 = CreateTopic(2, "Physics");

        db.Scopes.AddRange(d1, d2);
        await db.SaveChangesAsync();

        // No available items → both topics need generation
        itemService.Setup(s => s.NotRecentlySeenItems(123))
            .Returns(db.Items.Where(i => false));

        aiService.Setup(a => a.MakeQuestions(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(() => new List<Item>() { CreateValidItem() });

        await service.GenerateItemsTillThreshold(123);

        aiService.Verify(a => a.MakeQuestions(1, 1), Times.Once);
        aiService.Verify(a => a.MakeQuestions(2, 1), Times.Once);

        Assert.Equal(2, db.Items.Count());
        Assert.Equal(2, db.ScopeItems.Count());
    }

    [Fact]
    public async Task GenerateNewItems_WhenLevelResultNotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() => service.QueueNewItemGeneration(999));
    }

    [Fact]
    public async Task GenerateNewItems_WhenTotalNewItemsIsZero_DoesNotCallMakeQuestion()
    {
        // Arrange
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "Test" });
        db.Settings.Add(new Setting { Id = 20, LevelSize = 2, ProfileName = "Temp", TimeBeforeRedo = new TimeSpan(1, 0, 0, 0), LastActive = DateTime.UtcNow + new TimeSpan(0, 3, 0) });
        for (int i = 1; i <= 5; i++)
        {
            Item item = CreateValidItem();
            db.Items.Add(item);
            db.ScopeItems.Add(new ScopeItem { ItemId = i, ScopeId = 1 });
        }
        db.LevelResults.Add(new LevelResult { Id = 1, UserId = 1, CreatedAt = DateTime.UtcNow, TopicId = 1 });
        db.UserAnswers.Add(new UserAnswer { Id = 1, ItemId = 1, LevelResultId = 1, CompletionStatus = "completed", AnswerDate = DateTime.UtcNow, Correct = true });
        await db.SaveChangesAsync();

        var algorithmMock = new Mock<INewItemsAlgorithm>();
        algorithmMock
            .Setup(x => x.NewItemBatch(1, 1))
            .ReturnsAsync(new NewItemsRecord { TopicId = 1, UserId = 1, TotalNewItems = 0 });

        var registryMock = new Mock<IAlgorithmRegistry<INewItemsAlgorithm>>();
        registryMock.Setup(x => x.Get("Simple NIA")).Returns(algorithmMock.Object);

        Mock<IEventQueue> queue = new Mock<IEventQueue>();

        var localService = new ItemPoolService(db, aiService.Object, itemService.Object, registryMock.Object, logger.Object, queue.Object);

        // Act
        await localService.QueueNewItemGeneration(1);

        // Assert
        queue.Verify(x => x.QueueAsync(It.IsAny<EventData>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateNewItems_WhenTotalNewItemsIsThree_QueuesNewItemsWithThreeQuestions()
    {
        // Arrange
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "Test" });
        db.Settings.Add(new Setting { Id = 20, LevelSize = 2, ProfileName = "Temp", TimeBeforeRedo = new TimeSpan(1, 0, 0, 0), LastActive = DateTime.UtcNow + new TimeSpan(0, 3, 0) });
        for (int i = 1; i <= 5; i++)
        {
            Item item = CreateValidItem();
            db.Items.Add(item);
            db.ScopeItems.Add(new ScopeItem { ItemId = i, ScopeId = 1 });
        }
        db.LevelResults.Add(new LevelResult { Id = 1, UserId = 1, CreatedAt = DateTime.UtcNow, TopicId = 1 });
        db.UserAnswers.Add(new UserAnswer { Id = 1, ItemId = 1, LevelResultId = 1, CompletionStatus = "completed", AnswerDate = DateTime.UtcNow, Correct = true });
        await db.SaveChangesAsync();

        NewItemsRecord newItems = new NewItemsRecord { TopicId = 1, UserId = 1, TotalNewItems = 3 };
        var algorithmMock = new Mock<INewItemsAlgorithm>();
        algorithmMock
            .Setup(x => x.NewItemBatch(1, 1))
            .ReturnsAsync(newItems);

        var registryMock = new Mock<IAlgorithmRegistry<INewItemsAlgorithm>>();
        registryMock.Setup(x => x.Get("Simple NIA")).Returns(algorithmMock.Object);

        Mock<IEventQueue> queue = new Mock<IEventQueue>();

        var localService = new ItemPoolService(db, aiService.Object, itemService.Object, registryMock.Object, logger.Object, queue.Object);

        // Act
        await localService.QueueNewItemGeneration(1);

        // Assert
        queue.Verify(x => x.QueueAsync(
            It.Is<EventData>(e => e.EventType == EventType.ItemGeneration && e.Data == newItems),
            It.IsAny<CancellationToken>()
        ), Times.Exactly(1));
    }
}
