/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.UrningsAlgorithm;
using Models;
using Xunit;

namespace AATests.UrningsAlgorithm;

public class UrningsAlgorithmTests
{
    private readonly AppDbContext _db;
    private readonly BasicUrningsAlgorithm _algorithm;
    private const int LowBall = 0;
    private const int HighBall = 10; 
    
    public UrningsAlgorithmTests()
    {
        _db = SqliteInMemoryContextFactory.Create();
        _algorithm = new BasicUrningsAlgorithm(_db);
    }
    
    [Fact]
    public async Task UpdateUrnings_WhenUserTopicProgressNotFound_Throws()
    {
        var topic = new Scope { Id = 1, Name = "Test Topic" };
        _db.Scopes.Add(topic);
        
        _db.Items.Add(new Item
        {
            Id = 1,
            Active = true,
            Type = Item.ItemType.MultipleChoice,
            Source = Item.ItemSource.LLM,
            Lang = Item.Language.English,
            Scopes = [topic],
            QuestionText = "I am so cool!!!!",
            ResponseType = "conceptual",
            AppearanceCount = 0,
            Level = "easy",
            Answers = new List<ItemAnswer>
            {
                new ItemAnswer { AnswerText = "A", Correct = true, AnswerIdentifier = "A" }
            },
        });

        _db.ItemUrns.Add(new ItemUrn
        {
            ItemId = 1,
            GreenBalls = 50,
            RedBalls = 50
        });
        
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<Exception>(
            () => _algorithm.UpdateUrnings(userId: 1, itemId: 1, actuallyGreen: true));

        Assert.Equal("User urn not found", ex.Message);
    }

    [Fact]
    public async Task UpdateUrnings_WhenItemNotFound_Throws()
    {
        var topic = new Scope { Id = 1, Name = "Test Topic" };
        _db.Scopes.Add(topic);
        
        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DisplayName =  "John Doe",
            UserName = "testuser",
            NormalizedUserName = "TESTUSER",
            Email = "test@test.com",
            NormalizedEmail = "TEST@TEST.COM",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        _db.Users.Add(user);

        _db.UserScopeProgress.Add(new UserScopeProgress
        {
            UserId = 1, 
            ScopeId = 1, 
            Proficiency = 0, 
            Mastered = false,
            GreenBalls = HighBall,
            RedBalls = HighBall
        });
        
        // No Items defined, so there will be no items found. 
        
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<Exception>(
            () => _algorithm.UpdateUrnings(userId: 1, itemId: 1, actuallyGreen: true));

        Assert.Equal("Item urn not found", ex.Message);
    }

    /// <summary>
    /// User correct with green didn't predict green changes urnings 
    /// </summary>
    [Fact]
    public async Task UpdateUrnings_WhenPredictedCorrectButAnswerWasWrong_TransfersBallsToItem()
    {
        SeedUrns(userGreen: HighBall, userRed: LowBall, itemGreen: LowBall, itemRed: HighBall);
        await _db.SaveChangesAsync();

        await _algorithm.UpdateUrnings(userId: 1, itemId: 1, actuallyGreen: false);

        var userUrn = _db.UserScopeProgress.Single();
        var itemUrn = _db.ItemUrns.Single();
        Assert.Equal(HighBall - 1, userUrn.GreenBalls);
        Assert.Equal(LowBall  + 1, userUrn.RedBalls);
        Assert.Equal(LowBall  + 1, itemUrn.GreenBalls);
        Assert.Equal(HighBall - 1, itemUrn.RedBalls);
    }

    /// <summary>
    /// When user was right, but prediction was wrong, then user gets balls. 
    /// </summary>
    [Fact]
    public async Task UpdateUrnings_WhenPredictedWrongButUserWasRight_TransfersBallsToUser()
    {
        SeedUrns(userGreen: LowBall, userRed: HighBall, itemGreen: HighBall, itemRed: LowBall);
        await _db.SaveChangesAsync();

        await _algorithm.UpdateUrnings(userId: 1, itemId: 1, actuallyGreen: true);

        var userUrn = _db.UserScopeProgress.Single();
        var itemUrn = _db.ItemUrns.Single();
        Assert.Equal(LowBall  + 1, userUrn.GreenBalls);
        Assert.Equal(HighBall - 1, userUrn.RedBalls);
        Assert.Equal(HighBall - 1, itemUrn.GreenBalls);
        Assert.Equal(LowBall  + 1, itemUrn.RedBalls);
    }

    /// <summary>
    /// When prediction matches Reality, nothing changes
    /// </summary>
    [Theory]
    [InlineData(LowBall, HighBall, false)] 
    [InlineData(HighBall, LowBall, true)]
    public async Task UpdateUrnings_WhenPredictionMatchesReality_BallsDoNotChange(int val1, int val2, bool reality)
    {
        SeedUrns(userGreen: val1, userRed: val2, itemGreen: val2, itemRed: val1);
        await _db.SaveChangesAsync();

        await _algorithm.UpdateUrnings(userId: 1, itemId: 1, actuallyGreen: reality);

        var userUrn = _db.UserScopeProgress.Single();
        var itemUrn = _db.ItemUrns.Single();
        Assert.Equal(val1,  userUrn.GreenBalls);
        Assert.Equal(val2, userUrn.RedBalls);
        Assert.Equal(val2, itemUrn.GreenBalls);
        Assert.Equal(val1,  itemUrn.RedBalls);
    }
    

    /// <summary>
    /// When both urns are saturated (fully green or fully red) nothing can change. 
    /// </summary>
    [Theory]
    [InlineData(LowBall, HighBall)]
    [InlineData(HighBall, LowBall)]
    public async Task UpdateUrnings_WhenBothUrnsFullySaturated_CompletesWithoutChangingBalls(int val1, int val2)
    {
        SeedUrns(userGreen: val1, userRed: val2, itemGreen: val1, itemRed: val2);
        await _db.SaveChangesAsync();

        var ex = await Record.ExceptionAsync(
            () => _algorithm.UpdateUrnings(userId: 1, itemId: 1, actuallyGreen: false));

        Assert.Null(ex);
        Assert.Equal(val2, _db.UserScopeProgress.Single().RedBalls);
        Assert.Equal(val2, _db.ItemUrns.Single().RedBalls);
    }
    
    private void SeedUrns(int userGreen, int userRed, int itemGreen, int itemRed)
    {
        // Because of foreign key restraints, we need to make a topic and user first. 
        // This user also has a bunch of constraints. 
        
        var topic = new Scope { Id = 1, Name = "Test Topic" };
        _db.Scopes.Add(topic);

        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DisplayName =  "John Doe",
            UserName = "testuser",
            NormalizedUserName = "TESTUSER",
            Email = "test@test.com",
            NormalizedEmail = "TEST@TEST.COM",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        _db.Users.Add(user);

        _db.UserScopeProgress.Add(new UserScopeProgress
        {
            UserId = 1,
            ScopeId = 1,
            Proficiency = 0,
            Mastered = false,
            GreenBalls = userGreen,
            RedBalls = userRed
        });

        _db.Items.Add(new Item
        {
            Id = 1,
            Active = true,
            Type = Item.ItemType.MultipleChoice,
            Source = Item.ItemSource.LLM,
            Lang = Item.Language.English,
            Scopes = [topic],
            QuestionText = "I am so cool!!!!",
            ResponseType = "conceptual",
            AppearanceCount = 0,
            Level = "easy",
            Answers = new List<ItemAnswer>
            {
                new ItemAnswer { AnswerText = "A", Correct = true, AnswerIdentifier = "A" }
            },
        });

        _db.ItemUrns.Add(new ItemUrn
        {
            ItemId = 1,
            GreenBalls = itemGreen,
            RedBalls = itemRed
        });

        _db.SaveChanges();
    }
}