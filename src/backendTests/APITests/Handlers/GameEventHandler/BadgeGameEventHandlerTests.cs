/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA;
using AA.StampAlgorithm;
using API.Handlers.GameEventHandlers;
using API.Services;
using API.Tools;
using API.Tools.Badges;
using API.Tools.Badges.BadgeImageCollecting;
using APITests.Tools;
using APITests.Tools.Badges.FakeBadgeImages;
using Microsoft.Extensions.Logging;
using Models;
using Moq;

namespace APITests.Handlers.GameEventHandler;

public class BadgeGameEventHandlerTests
{
    private readonly AppDbContext _db;
    private readonly BadgeGameEventHandler _handler;
    private readonly Mock<IBadgeImageCollector> _collector;

    private readonly static DateTime fakeUtc = new DateTime(2024, 7, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly TimeTool timeTool = new TimeTool(new FakeTimeProvider(fakeUtc));

    public BadgeGameEventHandlerTests()
    {
        _db = SqliteInMemoryContextFactory.Create();
        _db.Users.AddRange(CreateUser(98), CreateUser(3), CreateUser(6));
        _db.SaveChanges();
        _collector = new Mock<IBadgeImageCollector>();
        
        var algorithmMock = new Mock<IStampAlgorithm>();
        algorithmMock
            .Setup(a => a.FindNewStampPosition(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(CreateStamp());
            
        var registryMock = new Mock<IAlgorithmRegistry<IStampAlgorithm>>();
        registryMock
            .Setup(r => r.Get(It.IsAny<string>()))
            .Returns(algorithmMock.Object);
        _handler = new BadgeGameEventHandler(_db, _collector.Object, registryMock.Object, timeTool, Mock.Of<ILogger<BadgeGameEventHandler>>(), Mock.Of<IAbTestingService>());
    }

    [Fact]
    public async Task GameEventTriggered_SimpleProgressUpdateCheck()
    {
        await CreateBadge("fake 1_20", 20, [GameEvent.ItemAnswered]);
        _db.BadgeProgresses.Add(new BadgeProgress { UserId = 3, BadgeIdentifier = "fake 1_20", Progress = 8 });
        await _db.SaveChangesAsync();
        _collector.Setup(c => c.GetBadgeImageCollection()).Returns(
            GetCollection(new FakeItemCompleted {ItemAmount = 20})
        );
        
        await _handler.GameEventTriggered(new FakeItemCorrectData { UserId = 98, Correct = true });
        await _handler.GameEventTriggered(new FakeItemCorrectData { UserId = 3, Correct = true });
        await _handler.GameEventTriggered(new FakeItemCorrectData { UserId = 3, Correct = false });
        
        Assert.Equal(1, (await _db.BadgeProgresses.FindAsync(98, "fake 1_20"))?.Progress);
        Assert.Equal(9, (await _db.BadgeProgresses.FindAsync(3, "fake 1_20"))?.Progress);
    }

    [Fact]
    public async Task GameEventTriggered_MultipleBadgesTriggered()
    {
        await CreateBadge("fake 1_20", 20, [GameEvent.ItemAnswered]);
        await CreateBadge("fake 1_30", 30, [GameEvent.ItemAnswered]);
        await CreateBadge("fake 1_40", 40, [GameEvent.ItemAnswered]);
        await CreateBadge("fake 2_30_80", 30 * 80, [GameEvent.ItemAnswered, GameEvent.StreakProlonged]);
        await CreateBadge("fake 2_60_70", 60 * 70, [GameEvent.ItemAnswered, GameEvent.StreakProlonged]);
        
        _db.BadgeProgresses.Add(new BadgeProgress { UserId = 3, BadgeIdentifier = "fake 1_30", Progress = 11 });
        _db.BadgeProgresses.Add(new BadgeProgress { UserId = 3, BadgeIdentifier = "fake 1_40", Progress = 33 });
        _db.BadgeProgresses.Add(new BadgeProgress { UserId = 3, BadgeIdentifier = "fake 2_60_70", Progress = 32 });
        await _db.SaveChangesAsync();
        
        _collector.Setup(c => c.GetBadgeImageCollection()).Returns(
            GetCollection(new FakeItemCompleted {ItemAmount = 20}, 
                new FakeItemCompleted {ItemAmount = 30},
                new FakeItemCompleted {ItemAmount = 40},
                new FakeRandom {ItemAmount = 30, StreakCount = 80},
                new FakeRandom {ItemAmount = 60, StreakCount = 70}
            )
        );
        
        await _handler.GameEventTriggered(new FakeItemCorrectData { UserId = 3, Correct = true });
        
        Assert.Equal(1, (await _db.BadgeProgresses.FindAsync(3, "fake 1_20"))?.Progress);
        Assert.Equal(12, (await _db.BadgeProgresses.FindAsync(3, "fake 1_30"))?.Progress);
        Assert.Equal(34, (await _db.BadgeProgresses.FindAsync(3, "fake 1_40"))?.Progress);
        Assert.Equal(4, (await _db.BadgeProgresses.FindAsync(3, "fake 2_30_80"))?.Progress);
        Assert.Equal(1024, (await _db.BadgeProgresses.FindAsync(3, "fake 2_60_70"))?.Progress);
    }

    [Fact]
    public async Task GameEventTriggered_ShouldCompleteWhenThresholdIsMet()
    {
        await CreateBadge("fake 1_40", 40, [GameEvent.ItemAnswered]);
        _db.BadgeProgresses.Add(new BadgeProgress { UserId = 3, BadgeIdentifier = "fake 1_40", Progress = 39 });
        await _db.SaveChangesAsync();
        _collector.Setup(c => c.GetBadgeImageCollection()).Returns(
            GetCollection(new FakeItemCompleted {ItemAmount = 40})
        );
        
        await _handler.GameEventTriggered(new FakeItemCorrectData { UserId = 3, Correct = true });

        BadgeProgress progress = (await _db.BadgeProgresses.FindAsync(3, "fake 1_40"))!;
        Assert.Equal(40, progress.Progress);
        Assert.NotNull(progress.StampId);
    }
    
    [Fact]
    public async Task GameEventTriggered_ShouldNotHandleAlreadyCompleted()
    {
        await CreateBadge("fake 1_40", 40, [GameEvent.ItemAnswered]);
        _db.BadgeProgresses.Add(new BadgeProgress { UserId = 3, BadgeIdentifier = "fake 1_40", Progress = 40, Stamp = CreateStamp()});
        _db.BadgeProgresses.Add(new BadgeProgress { UserId = 6, BadgeIdentifier = "fake 1_40", Progress = 60, Stamp = CreateStamp() });
        await _db.SaveChangesAsync();
        _collector.Setup(c => c.GetBadgeImageCollection()).Returns(
            GetCollection(new FakeItemCompleted {ItemAmount = 40})
        );
        
        await _handler.GameEventTriggered(new FakeItemCorrectData { UserId = 3, Correct = true });
        await _handler.GameEventTriggered(new FakeItemCorrectData { UserId = 6, Correct = true });

        BadgeProgress progressUser3 = (await _db.BadgeProgresses.FindAsync(3, "fake 1_40"))!;
        Assert.Equal(40, progressUser3.Progress);
        Assert.NotNull(progressUser3.StampId);
        
        BadgeProgress progressUser6 = (await _db.BadgeProgresses.FindAsync(6, "fake 1_40"))!;
        Assert.Equal(60, progressUser6.Progress);
        Assert.NotNull(progressUser6.StampId);
    }
    
    [Fact]
    public async Task GameEventTriggered_ShouldOnlyHandleTheRightBadges()
    {
        (_, BadgeState badgeState40) = await CreateBadge("fake 1_40", 40, [GameEvent.ItemAnswered]);
        (_, BadgeState badgeState50) = await CreateBadge("fake 1_50", 50, [GameEvent.ItemAnswered]);
        (_, BadgeState badgeState60) = await CreateBadge("fake 1_60", 60, [GameEvent.ItemAnswered]);
        (_, BadgeState badgeState70) = await CreateBadge("fake 1_70", 70, [GameEvent.ItemAnswered]);
        await CreateBadge("fake 1_80", 80, [GameEvent.ItemAnswered]);
        _db.BadgeProgresses.Add(new BadgeProgress { UserId = 3, BadgeIdentifier = "fake 1_40", Progress = 15, Stamp = CreateStamp()});
        _db.BadgeProgresses.Add(new BadgeProgress { UserId = 3, BadgeIdentifier = "fake 1_50", Progress = 20, Stamp = CreateStamp() });
        badgeState40.Phase = BadgePhase.Staging;
        badgeState50.Excluded = true;
        badgeState60.OpenFrom = timeTool.Now().AddDays(1);
        badgeState70.OpenUntil = timeTool.Now().AddMonths(-1);
        await _db.SaveChangesAsync();
        
        _collector.Setup(c => c.GetBadgeImageCollection()).Returns(
            GetCollection(
                new FakeItemCompleted {ItemAmount = 40},
                new FakeItemCompleted {ItemAmount = 50},
                new FakeItemCompleted {ItemAmount = 60},
                new FakeItemCompleted {ItemAmount = 70},
                new FakeItemCompleted {ItemAmount = 80}
            )
        );
        
        await _handler.GameEventTriggered(new FakeItemCorrectData { UserId = 3, Correct = true });

        Assert.Equal(15, (await _db.BadgeProgresses.FindAsync(3, "fake 1_40"))?.Progress);
        Assert.Equal(20, (await _db.BadgeProgresses.FindAsync(3, "fake 1_50"))?.Progress);
        Assert.Null((await _db.BadgeProgresses.FindAsync(3, "fake 1_60"))?.Progress);
        Assert.Null((await _db.BadgeProgresses.FindAsync(3, "fake 1_70"))?.Progress);
        Assert.Equal(1, (await _db.BadgeProgresses.FindAsync(3, "fake 1_80"))?.Progress);
    }

    private async Task<(Badge, BadgeState)> CreateBadge(string identifier, int progressNeeded, List<GameEvent> triggerConditions)
    {
        Badge badge = _db.Badges.Add(new Badge { Identifier = identifier, ProgressNeeded = progressNeeded }).Entity;
        await _db.SaveChangesAsync();
        BadgeState badgeState = _db.BadgeStates.Add(new BadgeState { BadgeIdentifier = identifier, Phase = BadgePhase.Published}).Entity;
        foreach (var triggerCondition in triggerConditions)
        {
            _db.BadgeTriggerConditions.Add(new BadgeTriggerCondition { BadgeIdentifier = identifier, TriggerCondition = triggerCondition});   
        }
        await _db.SaveChangesAsync();

        return (badge, badgeState);
    }
    
    private Dictionary<string, BadgeImage> GetCollection(params BadgeImage[] badgeImages)
    {
        return badgeImages.ToDictionary(i => i.BadgeIdentifier());
    }

    private User CreateUser(int id)
    {
        return new User
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            DisplayName =  "John Doe",
            UserName = "testuser",
            NormalizedUserName = "TESTUSER" + id,
            Email = "test@test.com",
            NormalizedEmail = "TEST@TEST.COM" + id,
            SecurityStamp = Guid.NewGuid().ToString()
        };
    }

    private BadgeStamp CreateStamp()
    {
        return new BadgeStamp { DateAccomplished = timeTool.Now(), X = 1, Y = 1, Page = 1 };
    }
}