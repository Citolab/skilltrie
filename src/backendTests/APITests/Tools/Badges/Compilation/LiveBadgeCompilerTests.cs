/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Tools.Badges;
using API.Tools.Badges.BadgeImageCollecting;
using API.Tools.Badges.Compilation;
using APITests.Tools.Badges.FakeBadgeImages;
using Microsoft.Extensions.Logging;
using Models;
using Moq;

namespace APITests.Tools.Badges.Compilation;

public class LiveBadgeCompilerTests
{
    private readonly AppDbContext _db;
    private readonly LiveBadgeCompiler _compiler;

    public LiveBadgeCompilerTests()
    {
        _db = SqliteInMemoryContextFactory.Create();
        _compiler = new LiveBadgeCompiler(_db, Mock.Of<ILogger<LiveBadgeCompiler>>());
    }

    [Fact]
    public void AllBadgeImagesCollected_ShouldPopulateDatabaseWithBadgesAndStates()
    {
        Dictionary<string, BadgeImage> collection = GetCollection(
            new FakeItemCompleted { ItemAmount = 10 },
            new FakeItemCompleted { ItemAmount = 20 },
            new FakeRandom { ItemAmount = 30, StreakCount = 50 }
        );

        _compiler.AllBadgeImagesCollected(collection);

        Assert.NotNull(_db.Badges.Find("fake 1_10"));
        Assert.NotNull(_db.Badges.Find("fake 1_20"));
        Assert.NotNull(_db.Badges.Find("fake 2_30_50"));
        
        Assert.NotNull(_db.BadgeStates.Find("fake 1_10"));
        Assert.NotNull(_db.BadgeStates.Find("fake 1_20"));
        Assert.NotNull(_db.BadgeStates.Find("fake 2_30_50"));
    }
    
    [Fact]
    public void AllBadgeImagesCollected_ShouldPopulateDatabaseWithBadgesAndTriggers()
    {
        Dictionary<string, BadgeImage> collection = GetCollection(
            new FakeItemCompleted { ItemAmount = 10 },
            new FakeRandom { ItemAmount = 30, StreakCount = 50 }
        );

        _compiler.AllBadgeImagesCollected(collection);

        Assert.NotNull(_db.Badges.Find("fake 1_10"));
        Assert.NotNull(_db.Badges.Find("fake 2_30_50"));
        
        Assert.NotNull(_db.BadgeTriggerConditions.Find("fake 1_10", GameEvent.ItemAnswered));
        Assert.NotNull(_db.BadgeTriggerConditions.Find("fake 2_30_50", GameEvent.StreakProlonged));
        Assert.NotNull(_db.BadgeTriggerConditions.Find("fake 2_30_50", GameEvent.ItemAnswered));
    }

    [Fact]
    public async Task AllBadgeImagesCollected_ShouldDeleteTriggersWhenNotIncludedAnymore()
    {
        Dictionary<string, BadgeImage> collection = GetCollection(
            new FakeItemCompleted { ItemAmount = 20 },
            new FakeItemCompleted { ItemAmount = 30 }
        );
        
        await _db.Badges.AddRangeAsync(
            new Badge {Identifier = "fake 1_20", ProgressNeeded = 20, Description = "Cool description"}, 
            new Badge {Identifier = "fake 1_30", ProgressNeeded = 30}
        );
        
        await _db.SaveChangesAsync();
        
        await _db.BadgeTriggerConditions.AddRangeAsync(
            new BadgeTriggerCondition {BadgeIdentifier = "fake 1_20", TriggerCondition = GameEvent.ItemAnswered}, 
            new BadgeTriggerCondition {BadgeIdentifier = "fake 1_20", TriggerCondition = GameEvent.StreakProlonged},
            new BadgeTriggerCondition {BadgeIdentifier = "fake 1_30", TriggerCondition = GameEvent.TestCompleted}
        );
        
        await _db.BadgeStates.AddRangeAsync(
            new BadgeState {BadgeIdentifier = "fake 1_20"}, 
            new BadgeState {BadgeIdentifier = "fake 1_30"}
        );
        
        await _db.SaveChangesAsync();
        
        await _compiler.AllBadgeImagesCollected(collection);
        
        _db.ChangeTracker.Clear();
        Assert.NotNull(await _db.BadgeTriggerConditions.FindAsync("fake 1_20", GameEvent.ItemAnswered));
        Assert.NotNull(await _db.BadgeTriggerConditions.FindAsync("fake 1_30", GameEvent.ItemAnswered));
        Assert.Null(await _db.BadgeTriggerConditions.FindAsync("fake 1_20", GameEvent.StreakProlonged));
        Assert.Null(await _db.BadgeTriggerConditions.FindAsync("fake 1_30", GameEvent.TestCompleted));
    }
    
    [Fact]
    public void AllBadgeImagesCollected_MetadataShouldBeStored()
    {
        Dictionary<string, BadgeImage> collection = GetCollection(
            new FakeItemCompleted { ItemAmount = 10, CreatedFrom = new ParameterizedBadgeEntry {
                        Metadata = new Dictionary<string, object> 
                        { 
                            {"Description", "This is a cool description"}, 
                            {"Category", "This is a cool category"} 
                        }}},
            new FakeItemCompleted { ItemAmount = 20 },
            new FakeRandom { ItemAmount = 30, StreakCount = 50 }
        );

        _compiler.AllBadgeImagesCollected(collection);
        
        Assert.NotNull(_db.Badges.Find("fake 1_20"));
        Assert.NotNull(_db.Badges.Find("fake 2_30_50"));
        Assert.Equal("This is a cool category", _db.Badges.Find("fake 1_10")?.Category);
        Assert.Equal("This is a cool description", _db.Badges.Find("fake 1_10")?.Description);
    }

    [Fact]
    public async Task AllBadgeImagesCollected_ExistingRecordsShouldBeUpdated()
    {
        await _db.Badges.AddRangeAsync(
            new Badge {Identifier = "fake 1_20", ProgressNeeded = 20, Description = "Cool description"}, 
            new Badge {Identifier = "fake 1_30", ProgressNeeded = 30}
        );
        
        await _db.SaveChangesAsync();
        
        await _db.BadgeStates.AddRangeAsync(
            new BadgeState {BadgeIdentifier = "fake 1_20"}, 
            new BadgeState {BadgeIdentifier = "fake 1_30"}
        );
        
        await _db.SaveChangesAsync();
        
        Dictionary<string, BadgeImage> collection = GetCollection(
            new FakeItemCompleted { ItemAmount = 10 },
            new FakeItemCompleted { ItemAmount = 20, CreatedFrom = new ParameterizedBadgeEntry { Metadata = 
                new Dictionary<string, object> 
                {
                    {"Description", "Way cooler description"} 
                }}},
            new FakeItemCompleted { ItemAmount = 30 },
            new FakeRandom { ItemAmount = 30, StreakCount = 50 }
        );
        
        await _compiler.AllBadgeImagesCollected(collection);
        
        Assert.NotNull(await _db.Badges.FindAsync("fake 1_10"));
        Assert.Equal("Way cooler description", (await _db.Badges.FindAsync("fake 1_20"))?.Description);
        Assert.NotNull(await _db.Badges.FindAsync("fake 1_30"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 2_30_50"));
    }
    
    [Fact]
    public async Task AllBadgeImagesCollected_ExcludedRecordsShouldHaveExcludedProperty()
    {
        await _db.Badges.AddRangeAsync(
            new Badge {Identifier = "fake 1_20", ProgressNeeded = 20, Description = "Cool description"}, 
            new Badge {Identifier = "fake 2_30_77", ProgressNeeded = 8890},
            new Badge {Identifier = "fake 1_90"},
            new Badge {Identifier = "fake 3"}
        );
        
        await _db.SaveChangesAsync();
        
        await _db.BadgeStates.AddRangeAsync(
            new BadgeState {BadgeIdentifier = "fake 1_20"}, 
            new BadgeState {BadgeIdentifier = "fake 2_30_77"},
            new BadgeState {BadgeIdentifier = "fake 1_90", Excluded = true},
            new BadgeState {BadgeIdentifier = "fake 3"}
        );
        
        await _db.SaveChangesAsync();
        
        Dictionary<string, BadgeImage> collection = GetCollection(
            new FakeItemCompleted { ItemAmount = 10 },
            new FakeItemCompleted { ItemAmount = 80},
            new FakeItemCompleted { ItemAmount = 90 },
            new FakeRandom { ItemAmount = 30, StreakCount = 50 }
        );
        
        await _compiler.AllBadgeImagesCollected(collection);
        
        Assert.NotNull(await _db.Badges.FindAsync("fake 1_10"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 1_80"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 1_90"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 2_30_50"));
        
        Assert.True((await _db.BadgeStates.FindAsync("fake 1_20"))?.Excluded);
        Assert.True((await _db.BadgeStates.FindAsync("fake 2_30_77"))?.Excluded);
        Assert.True((await _db.BadgeStates.FindAsync("fake 3"))?.Excluded);
    }
    
    [Fact]
    public async Task AllBadgeImagesCollected_ReincludedBadgesShouldNotHaveExcludedProperty()
    {
        await _db.Badges.AddRangeAsync(
            new Badge {Identifier = "fake 1_20", ProgressNeeded = 20, Description = "Cool description"}, 
            new Badge {Identifier = "fake 2_30_77", ProgressNeeded = 8890},
            new Badge {Identifier = "fake 1_90"},
            new Badge {Identifier = "fake 1_95"},
            new Badge {Identifier = "fake 1_100"}
        );
        
        await _db.SaveChangesAsync();
        
        await _db.BadgeStates.AddRangeAsync(
            new BadgeState {BadgeIdentifier = "fake 1_20", Excluded = true}, 
            new BadgeState {BadgeIdentifier = "fake 2_30_77", Excluded = true},
            new BadgeState {BadgeIdentifier = "fake 1_90", Excluded = true},
            new BadgeState {BadgeIdentifier = "fake 1_95", Excluded = true},
            new BadgeState {BadgeIdentifier = "fake 1_100"}
        );
        
        await _db.SaveChangesAsync();
        
        Dictionary<string, BadgeImage> collection = GetCollection(
            new FakeItemCompleted { ItemAmount = 10 },
            new FakeItemCompleted { ItemAmount = 20 },
            new FakeItemCompleted { ItemAmount = 95 },
            new FakeItemCompleted { ItemAmount = 100 },
            new FakeRandom { ItemAmount = 800, StreakCount = 50 }
        );
        
        await _compiler.AllBadgeImagesCollected(collection);
        
        Assert.NotNull(await _db.Badges.FindAsync("fake 1_10"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 1_20"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 1_90"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 1_95"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 1_100"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 2_800_50"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 2_30_77"));
        
        Assert.False((await _db.BadgeStates.FindAsync("fake 1_20"))?.Excluded);
        Assert.True((await _db.BadgeStates.FindAsync("fake 2_30_77"))?.Excluded);
        Assert.True((await _db.BadgeStates.FindAsync("fake 1_90"))?.Excluded);
        Assert.False((await _db.BadgeStates.FindAsync("fake 1_95"))?.Excluded);
        Assert.False((await _db.BadgeStates.FindAsync("fake 1_100"))?.Excluded);
    }


    [Fact]
    public async Task BadgeImageAdded_ShouldAddCorrectly()
    {
        await _compiler.BadgeImageAdded(new FakeItemCompleted { ItemAmount = 10 });
        await _compiler.BadgeImageAdded(new FakeItemCompleted { ItemAmount = 20 });
        await _compiler.BadgeImageAdded(new FakeRandom { ItemAmount = 30, StreakCount = 80});

        Assert.NotNull(await _db.Badges.FindAsync("fake 1_10"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 1_20"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 2_30_80"));
        
        Assert.Equal(BadgePhase.Staging, (await _db.BadgeStates.FindAsync("fake 1_10"))?.Phase);
        Assert.Equal(BadgePhase.Staging, (await _db.BadgeStates.FindAsync("fake 1_20"))?.Phase);
        Assert.Equal(BadgePhase.Staging, (await _db.BadgeStates.FindAsync("fake 2_30_80"))?.Phase);
        
        Assert.NotNull(await _db.BadgeTriggerConditions.FindAsync("fake 1_10", GameEvent.ItemAnswered));
        Assert.NotNull(await _db.BadgeTriggerConditions.FindAsync("fake 1_20", GameEvent.ItemAnswered));
        Assert.NotNull(await _db.BadgeTriggerConditions.FindAsync("fake 2_30_80", GameEvent.StreakProlonged));
        Assert.NotNull(await _db.BadgeTriggerConditions.FindAsync("fake 2_30_80", GameEvent.ItemAnswered));
    }

    [Fact]
    public async Task BadgeImageAdded_ShouldDealCorrectlyWithExistingBadges()
    {
        await _db.Badges.AddRangeAsync(
            new Badge {Identifier = "fake 1_20", ProgressNeeded = 20, Description = "Cool description"}, 
            new Badge {Identifier = "fake 2_30_77", ProgressNeeded = 8890},
            new Badge {Identifier = "fake 1_90"},
            new Badge {Identifier = "fake 3"}
        );
        
        await _db.SaveChangesAsync();
        
        await _db.BadgeStates.AddRangeAsync(
            new BadgeState {BadgeIdentifier = "fake 1_20", Phase = BadgePhase.Published}, 
            new BadgeState {BadgeIdentifier = "fake 2_30_77", Phase = BadgePhase.Disabled},
            new BadgeState {BadgeIdentifier = "fake 1_90", Excluded = true, Phase = BadgePhase.Published},
            new BadgeState {BadgeIdentifier = "fake 3"}
        );
        
        await _db.SaveChangesAsync();

        await _compiler.BadgeImageAdded(new FakeItemCompleted { ItemAmount = 20 });
        await _compiler.BadgeImageAdded(new FakeRandom { ItemAmount = 30, StreakCount = 77});
        await _compiler.BadgeImageAdded(new FakeItemCompleted { ItemAmount = 90 });
        await _compiler.BadgeImageAdded(new FakeWithoutParams());
        
        Assert.Equal(BadgePhase.Staging, (await _db.BadgeStates.FindAsync("fake 1_20"))?.Phase);
        Assert.Equal(BadgePhase.Staging, (await _db.BadgeStates.FindAsync("fake 1_90"))?.Phase);
        Assert.Equal(BadgePhase.Staging, (await _db.BadgeStates.FindAsync("fake 2_30_77"))?.Phase);
        Assert.Equal(BadgePhase.Staging, (await _db.BadgeStates.FindAsync("fake 3"))?.Phase);
        
        Assert.False((await _db.BadgeStates.FindAsync("fake 1_20"))?.Excluded);
        Assert.False((await _db.BadgeStates.FindAsync("fake 1_90"))?.Excluded);
        Assert.False((await _db.BadgeStates.FindAsync("fake 2_30_77"))?.Excluded);
        Assert.False((await _db.BadgeStates.FindAsync("fake 3"))?.Excluded);
    }
    
    [Fact]
    public async Task BadgeImageRemoved_ShouldRemoveCorrectly()
    {
        await _db.Badges.AddRangeAsync(
            new Badge {Identifier = "fake 1_20", ProgressNeeded = 20, Description = "Cool description"}, 
            new Badge {Identifier = "fake 2_30_77", ProgressNeeded = 8890},
            new Badge {Identifier = "fake 1_90"},
            new Badge {Identifier = "fake 3"}
        );
        
        await _db.SaveChangesAsync();
        
        await _db.BadgeStates.AddRangeAsync(
            new BadgeState {BadgeIdentifier = "fake 1_20", Phase = BadgePhase.Published}, 
            new BadgeState {BadgeIdentifier = "fake 2_30_77", Phase = BadgePhase.Disabled},
            new BadgeState {BadgeIdentifier = "fake 1_90", Excluded = true, Phase = BadgePhase.Published},
            new BadgeState {BadgeIdentifier = "fake 3"}
        );
        
        await _db.SaveChangesAsync();
        
        await _compiler.BadgeImageRemoved(new FakeItemCompleted { ItemAmount = 20 });
        await _compiler.BadgeImageRemoved(new FakeItemCompleted { ItemAmount = 90 });
        
        Assert.Null(await _db.Badges.FindAsync("fake 1_20"));
        Assert.Null(await _db.Badges.FindAsync("fake 1_90"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 2_30_77"));
        Assert.NotNull(await _db.Badges.FindAsync("fake 3"));
    }
    
    private Dictionary<string, BadgeImage> GetCollection(params BadgeImage[] badgeImages)
    {
        return badgeImages.ToDictionary(i => i.BadgeIdentifier());
    }
}