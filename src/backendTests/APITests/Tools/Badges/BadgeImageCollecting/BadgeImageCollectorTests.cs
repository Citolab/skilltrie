/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Tools.Badges;
using API.Tools.Badges.BadgeImageCollecting;
using APITests.Tools.Badges.FakeBadgeImages;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using Moq;

namespace APITests.Tools.Badges.BadgeImageCollecting;

public class BadgeImageCollectorTests
{
    private readonly AppDbContext _db;
    private readonly BadgeImageCollector _badgeImageCollector;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    public BadgeImageCollectorTests()
    {
        _db = SqliteInMemoryContextFactory.Create();
        _db.Scopes.AddRange(
            new Scope {Id = 1, Name = "T1"},
            new Scope {Id = 2, Name = "T2"},
            new Scope {Id = 3, Name = "T3"}
        );
        Mock<IServiceScopeFactory> scopeFactoryMock = new Mock<IServiceScopeFactory>();
        Mock<IServiceScope> scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        
        _serviceProviderMock
            .Setup(s => s.GetService(typeof(AppDbContext)))
            .Returns(_db);
        
        _serviceProviderMock
            .Setup(s => s.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);
        
        scopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(_serviceProviderMock.Object);
        
        scopeFactoryMock
            .Setup(s => s.CreateScope())
            .Returns(scopeMock.Object);
        
        _badgeImageCollector = new BadgeImageCollector(_serviceProviderMock.Object, Mock.Of<ILogger<BadgeImageCollector>>());
        _badgeImageCollector.Assembly = typeof(BadgeImageCollectorTests).Assembly;
    }

    [Fact]
    public void CollectAllBadgeImages_SimpleTestWithoutParameterization()
    {
        _badgeImageCollector.CollectAllBadgeImages();
        AssertBadgeCollectionEquivalent(new Dictionary<string, BadgeImage>
        {
            {"fake 3", new FakeWithoutParams()},
            {"fake 4", new FakeWithoutParams2()},
        }, _badgeImageCollector.GetBadgeImageCollection());
    }

    [Fact]
    public async Task CollectAllBadgeImages_SimpleParameterizationTest()
    {
        _db.ParameterizedBadgeEntries.AddRange(
            new ParameterizedBadgeEntry {BadgeImage = "FakeItemCompleted", Amount = 10},
            new ParameterizedBadgeEntry {BadgeImage = "FakeItemCompleted", Amount = 50},
            new ParameterizedBadgeEntry {BadgeImage = "FakeItemCompleted", Amount = 100},
            new ParameterizedBadgeEntry {BadgeImage = "FakeRandom", Amount = 1, StreakCount = 5},
            new ParameterizedBadgeEntry {BadgeImage = "FakeRandom", Amount = 2, StreakCount = 2000}
        );
        await _db.SaveChangesAsync();
        
        await _badgeImageCollector.CollectAllBadgeImages();
        Dictionary<string, BadgeImage> badgeCollection = _badgeImageCollector.GetBadgeImageCollection();
        AssertBadgeCollectionEquivalent(new Dictionary<string, BadgeImage>
        {
            {"fake 1_10", new FakeItemCompleted {ItemAmount = 10}},
            {"fake 1_50", new FakeItemCompleted {ItemAmount = 50}},
            {"fake 1_100", new FakeItemCompleted {ItemAmount = 100}},
            {"fake 2_1_5", new FakeRandom {ItemAmount = 1, StreakCount = 5}},
            {"fake 2_2_2000", new FakeRandom {ItemAmount = 2, StreakCount = 2000}},
            {"fake 3", new FakeWithoutParams()},
            {"fake 4", new FakeWithoutParams2()}
        }, badgeCollection);
    }
    
    [Fact]
    public async Task CollectAllBadgeImages_ParameterizationTestReferences()
    {
        _db.ParameterizedBadgeEntries.AddRange(
            new ParameterizedBadgeEntry {BadgeImage = "FakeTopicMastered", ScopeId = 1},
            new ParameterizedBadgeEntry {BadgeImage = "FakeTopicMastered", ScopeId = 2},
            new ParameterizedBadgeEntry {BadgeImage = "FakeItemCompleted", Amount = 100}
        );
        await _db.SaveChangesAsync();
        
        await _badgeImageCollector.CollectAllBadgeImages();
        Dictionary<string, BadgeImage> badgeCollection = _badgeImageCollector.GetBadgeImageCollection();
        
        AssertBadgeCollectionEquivalent(new Dictionary<string, BadgeImage>
        {
            {"fake 1_100", new FakeItemCompleted {ItemAmount = 100}},
            {"fake topic_T1", new FakeTopicMastered {Topic = await _db.Scopes.FirstAsync(t => t.Id == 1)}},
            {"fake topic_T2", new FakeTopicMastered {Topic = await _db.Scopes.FirstAsync(t => t.Id == 2)}},
            {"fake 3", new FakeWithoutParams()},
            {"fake 4", new FakeWithoutParams2()}
        }, badgeCollection);
    }
    
    [Fact]
    public async Task CollectAllBadgeImages_CorrectMetadata()
    {
        _db.ParameterizedBadgeEntries.AddRange(
            new ParameterizedBadgeEntry {BadgeImage = "FakeItemCompleted", Amount = 8, Metadata = new Dictionary<string, object>
            {
                {"Description", "some description"},
                {"DisplayName", "display name"}
            }}
        );
        await _db.SaveChangesAsync();
        
        await _badgeImageCollector.CollectAllBadgeImages();
        Dictionary<string, BadgeImage> badgeCollection = _badgeImageCollector.GetBadgeImageCollection();
        
        badgeCollection.GetValueOrDefault("fake 1_8")?.CreatedFrom?.Metadata.Should()
            .BeEquivalentTo(new Dictionary<string, object>
            {
                {"Description", "some description"},
                {"DisplayName", "display name"}
            });
        
        badgeCollection.Keys.Should().BeEquivalentTo("fake 1_8", "fake 3", "fake 4");
    }

    [Fact]
    public async Task CollectAllBadgeImages_ShouldSkipEntryWhenRequiredParameterIsMissing()
    {
        _db.ParameterizedBadgeEntries.AddRange(
            new ParameterizedBadgeEntry {BadgeImage = "FakeItemCompleted"}, // Too little properties is not okay
            new ParameterizedBadgeEntry {BadgeImage = "FakeItemCompleted", Amount = 10},
            new ParameterizedBadgeEntry {BadgeImage = "FakeItemCompleted", Amount = 50, StreakCount = 12}, // Too many properties is okay
            new ParameterizedBadgeEntry {BadgeImage = "FakeRandom", Amount = 1},
            new ParameterizedBadgeEntry {BadgeImage = "FakeRandom", Amount = 2, StreakCount = 198}
        );
        await _db.SaveChangesAsync();
        
        await _badgeImageCollector.CollectAllBadgeImages();
        Dictionary<string, BadgeImage> badgeCollection = _badgeImageCollector.GetBadgeImageCollection();
        
        AssertBadgeCollectionEquivalent(new Dictionary<string, BadgeImage>
        {
            {"fake 1_10", new FakeItemCompleted { ItemAmount = 10 }},
            {"fake 1_50", new FakeItemCompleted { ItemAmount = 50 }},
            {"fake 2_2_198", new FakeRandom { ItemAmount = 2, StreakCount = 198}},
            {"fake 3", new FakeWithoutParams()},
            {"fake 4", new FakeWithoutParams2()}
        }, badgeCollection);
    }
    
    [Fact]
    public async Task CollectAllBadgeImages_ShouldSkipFaultyBadgeImages()
    {
        _db.ParameterizedBadgeEntries.AddRange(
            new ParameterizedBadgeEntry {BadgeImage = "Faulty", StreakCount = 20},
            new ParameterizedBadgeEntry {BadgeImage = "Faulty2", Amount = 10, StreakCount = 30}
        );
        await _db.SaveChangesAsync();
        
        await _badgeImageCollector.CollectAllBadgeImages();
        Dictionary<string, BadgeImage> badgeCollection = _badgeImageCollector.GetBadgeImageCollection();
        
        AssertBadgeCollectionEquivalent(new Dictionary<string, BadgeImage>
        {
            {"fake 3", new FakeWithoutParams()},
            {"fake 4", new FakeWithoutParams2()}
        }, badgeCollection);
    }

    class FakeSubscriber(AppDbContext context) : IBadgeImageCollectorListener
    {
        public bool Added = false;
        public bool Removed = false;
        
        public async Task AllBadgeImagesCollected(Dictionary<string, BadgeImage> collectedBadgeImages)
        {
            context.Badges.Add(collectedBadgeImages.First().Value.Compile());
            await context.SaveChangesAsync();
        }

        public Task BadgeImageAdded(BadgeImage badgeImage)
        {
            Added = true;
            return Task.CompletedTask;
        }

        public Task BadgeImageRemoved(BadgeImage badgeImage)
        {
            Removed = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task CollectAllBadgeImages_SubscriptionWorks()
    {
        SetupSubscribers(new FakeSubscriber(_db));
        
        await _badgeImageCollector.CollectAllBadgeImages();
        
        Badge? badge = await _db.Badges.FindAsync("fake 3");
        Assert.NotNull(badge);
    }

    [Fact]
    public async Task ParameterizedEntryAdded_ShouldAddToCollection()
    {
        FakeSubscriber fakeSubscriber = new FakeSubscriber(_db);
        SetupSubscribers(fakeSubscriber);
        await SetupStandardCollection();

        await _badgeImageCollector.ParameterizedEntryAdded(new ParameterizedBadgeEntry
        {
            BadgeImage = "FakeItemCompleted", Amount = 60
        });
        await _badgeImageCollector.ParameterizedEntryAdded(new ParameterizedBadgeEntry
        {
            BadgeImage = "FakeItemCompleted", Amount = 70
        });
        await _badgeImageCollector.ParameterizedEntryAdded(new ParameterizedBadgeEntry
        {
            BadgeImage = "FakeRandom", Amount = 3, StreakCount = 198 
        });

        Dictionary<string, BadgeImage> badgeCollection = _badgeImageCollector.GetBadgeImageCollection();
        AssertBadgeCollectionEquivalent(new Dictionary<string, BadgeImage>
        {
            {"fake 1_10", new FakeItemCompleted { ItemAmount = 10 }},
            {"fake 1_50", new FakeItemCompleted { ItemAmount = 50 }},
            {"fake 2_2_198", new FakeRandom { ItemAmount = 2, StreakCount = 198}},
            {"fake 3", new FakeWithoutParams()},
            {"fake 4", new FakeWithoutParams2()},
            {"fake 1_60", new FakeItemCompleted { ItemAmount = 60 }},
            {"fake 1_70", new FakeItemCompleted { ItemAmount = 70 }},
            {"fake 2_3_198", new FakeRandom { ItemAmount = 3, StreakCount = 198}},
        }, badgeCollection);
        Assert.True(fakeSubscriber.Added);
    }
    
    [Fact]
    public async Task ParameterizedEntryAdded_ShouldThrowWhenTryingToAddDuplicateToCollection()
    {
        FakeSubscriber fakeSubscriber = new FakeSubscriber(_db);
        SetupSubscribers(fakeSubscriber);
        await SetupStandardCollection();

        await Assert.ThrowsAsync<Exception>(() => _badgeImageCollector.ParameterizedEntryAdded(new ParameterizedBadgeEntry
        {
            BadgeImage = "FakeItemCompleted", Amount = 50
        }));

        Dictionary<string, BadgeImage> badgeCollection = _badgeImageCollector.GetBadgeImageCollection();
        AssertBadgeCollectionEquivalent(new Dictionary<string, BadgeImage>
        {
            {"fake 1_10", new FakeItemCompleted { ItemAmount = 10 }},
            {"fake 1_50", new FakeItemCompleted { ItemAmount = 50 }},
            {"fake 2_2_198", new FakeRandom { ItemAmount = 2, StreakCount = 198}},
            {"fake 3", new FakeWithoutParams()},
            {"fake 4", new FakeWithoutParams2()},
        }, badgeCollection);
        Assert.False(fakeSubscriber.Added);
    }
    
    [Fact]
    public async Task ParameterizedEntryAdded_ShouldNotAddInvalidEntriesToCollection()
    {
        FakeSubscriber fakeSubscriber = new FakeSubscriber(_db);
        SetupSubscribers(fakeSubscriber);
        await SetupStandardCollection();

        await Assert.ThrowsAsync<Exception>(() => _badgeImageCollector.ParameterizedEntryAdded(new ParameterizedBadgeEntry
        {
            BadgeImage = "FakeRandom", Amount = 50
        }));

        Dictionary<string, BadgeImage> badgeCollection = _badgeImageCollector.GetBadgeImageCollection();
        AssertBadgeCollectionEquivalent(new Dictionary<string, BadgeImage>
        {
            {"fake 1_10", new FakeItemCompleted { ItemAmount = 10 }},
            {"fake 1_50", new FakeItemCompleted { ItemAmount = 50 }},
            {"fake 2_2_198", new FakeRandom { ItemAmount = 2, StreakCount = 198}},
            {"fake 3", new FakeWithoutParams()},
            {"fake 4", new FakeWithoutParams2()},
        }, badgeCollection);
        Assert.False(fakeSubscriber.Added);
    }

    [Fact]
    public async Task RemoveBadge_ShouldRemoveFromCollectionIfExists()
    {
        FakeSubscriber fakeSubscriber = new FakeSubscriber(_db);
        SetupSubscribers(fakeSubscriber);
        await SetupStandardCollection();
        
        await _badgeImageCollector.RemoveBadge(new FakeRandom
        {
            ItemAmount = 2, StreakCount = 198
        }.BadgeIdentifier());
        await _badgeImageCollector.RemoveBadge(new FakeItemCompleted
        {
            ItemAmount = 10
        }.BadgeIdentifier());
        await _badgeImageCollector.RemoveBadge(new FakeWithoutParams().BadgeIdentifier());

        Dictionary<string, BadgeImage> badgeCollection = _badgeImageCollector.GetBadgeImageCollection();
        AssertBadgeCollectionEquivalent(new Dictionary<string, BadgeImage>
        {
            {"fake 1_50", new FakeItemCompleted { ItemAmount = 50 }},
            {"fake 4", new FakeWithoutParams2()},
        }, badgeCollection);
        Assert.True(fakeSubscriber.Removed);
    }
    
    [Fact]
    public async Task RemoveBadge_ShouldNotRemoveWhenNotExists()
    {
        FakeSubscriber fakeSubscriber = new FakeSubscriber(_db);
        SetupSubscribers(fakeSubscriber);
        await SetupStandardCollection();
        
        await _badgeImageCollector.RemoveBadge(new FakeRandom
        {
            ItemAmount = 88, StreakCount = 198
        }.BadgeIdentifier());
        await _badgeImageCollector.RemoveBadge(new FakeItemCompleted
        {
            ItemAmount = 30
        }.BadgeIdentifier());
        await _badgeImageCollector.RemoveBadge(new FakeWithoutParams2().BadgeIdentifier());

        Dictionary<string, BadgeImage> badgeCollection = _badgeImageCollector.GetBadgeImageCollection();
        AssertBadgeCollectionEquivalent(new Dictionary<string, BadgeImage>
        {
            {"fake 1_10", new FakeItemCompleted { ItemAmount = 10 }},
            {"fake 1_50", new FakeItemCompleted { ItemAmount = 50 }},
            {"fake 2_2_198", new FakeRandom { ItemAmount = 2, StreakCount = 198}},
            {"fake 3", new FakeWithoutParams()},
        }, badgeCollection);
        Assert.True(fakeSubscriber.Removed);
    }

    private async Task SetupStandardCollection()
    {
        _db.ParameterizedBadgeEntries.AddRange(
            new ParameterizedBadgeEntry {BadgeImage = "FakeItemCompleted", Amount = 10},
            new ParameterizedBadgeEntry {BadgeImage = "FakeItemCompleted", Amount = 50},
            new ParameterizedBadgeEntry {BadgeImage = "FakeRandom", Amount = 2, StreakCount = 198}
        );
        await _db.SaveChangesAsync();
        await _badgeImageCollector.CollectAllBadgeImages();
    }

    private void SetupSubscribers(params IBadgeImageCollectorListener[] listeners)
    {
        _serviceProviderMock
            .Setup(s => s.GetService(typeof(IEnumerable<IBadgeImageCollectorListener>)))
            .Returns(listeners);
    }
    
    private void AssertBadgeCollectionEquivalent(Dictionary<string, BadgeImage> expectedCollection, Dictionary<string, BadgeImage> actualCollection)
    {
        foreach (var badgeImagePair in expectedCollection)
        {
            Assert.Equal(
                badgeImagePair.Value, 
                actualCollection.GetValueOrDefault(badgeImagePair.Key)
            );
        }
    
        Assert.Equal(expectedCollection.Keys.ToHashSet(), actualCollection.Keys.ToHashSet());
    }
}