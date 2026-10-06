/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Models;
using API.Handlers;
using API.Services;
using AA;
using AA.UserProficiencyAlgorithm;
using API.Controllers.DTOs;
using FluentAssertions;
using AA.NextTopicAlgorithm;
using AA.UserMasteryAlgorithm;
using Microsoft.Extensions.Logging;

namespace APITests.Services;

public class TopicServiceTests
{
    private static (AppDbContext Ctx, SqliteConnection Conn) CreateSqliteContext()
    {
        var conn = new SqliteConnection("Filename=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;

        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();

        // Seed users
        ctx.Users.AddRange(
            new User { Id = 1, FirstName = "John" , LastName = "Pork", Email = "john@test.com" , UserName = "john@test.com" , DisplayName = "JohnPork" },
            new User { Id = 2, FirstName = "Sara" , LastName = "Pork", Email = "sara@test.com" , UserName = "sara@test.com" , DisplayName = "SaraPork" },
            new User { Id = 3, FirstName = "Timmy", LastName = "Pork", Email = "timmy@test.com", UserName = "timmy@test.com", DisplayName = "TimmyPork" }
        );

        // Seed topics
        ctx.Scopes.AddRange(
            new Scope { Id = 185, Name = "statistics"            , Type = ScopeType.Subject },
            new Scope { Id = 1  , Name = "descriptive statistics", Type = ScopeType.Domain },
            new Scope { Id = 2  , Name = "mean"                  , Type = ScopeType.Topic },
            new Scope { Id = 3  , Name = "standard deviation"    , Type = ScopeType.Topic }
            );

        ctx.ScopeMemberships.AddRange(
            new ScopeMembership { AncestorId = 185, DescendantId = 1, Depth = 0 },
            new ScopeMembership { AncestorId = 1  , DescendantId = 2, Depth = 0 },
            new ScopeMembership { AncestorId = 1  , DescendantId = 3, Depth = 0 }
        );

        ctx.ScopeEdges.AddRange(
            new ScopeEdge() { FromScopeId = 2, ToScopeId = 3 }
        );

        // Seed user proficiencies
        ctx.UserScopeProgress.AddRange(
            new UserScopeProgress { UserId = 1, ScopeId = 185, Proficiency = 0, Mastered = false },
            new UserScopeProgress { UserId = 1, ScopeId = 1  , Proficiency = 0, Mastered = false },
            new UserScopeProgress { UserId = 1, ScopeId = 2  , Proficiency = 1, Mastered = true },
            new UserScopeProgress { UserId = 1, ScopeId = 3  , Proficiency = 1, Mastered = false },
            new UserScopeProgress { UserId = 2, ScopeId = 185, Proficiency = 0, Mastered = false },
            new UserScopeProgress { UserId = 2, ScopeId = 1, Proficiency = 0, Mastered = false },
            new UserScopeProgress { UserId = 2, ScopeId = 2, Proficiency = 1, Mastered = false },
            new UserScopeProgress { UserId = 2, ScopeId = 3, Proficiency = 1, Mastered = false },
            new UserScopeProgress { UserId = 3, ScopeId = 185, Proficiency = 0, Mastered = false },
            new UserScopeProgress { UserId = 3, ScopeId = 1, Proficiency = 0, Mastered = false },
            new UserScopeProgress { UserId = 3, ScopeId = 2, Proficiency = 1, Mastered = true },
            new UserScopeProgress { UserId = 3, ScopeId = 3, Proficiency = 1, Mastered = true }
            );

        UserAnswer uaTrue  = new() { Id = 1, LevelResultId = 1, Correct = true,  Item = new Item() { Id = 1, QuestionText = "yo", ResponseType = "gurt" } };
        UserAnswer uaFalse = new() { Id = 2, LevelResultId = 2, Correct = false, Item = new Item() { Id = 2, QuestionText = "yo", ResponseType = "gurt" } };

        ctx.LevelResults.AddRange(
            new LevelResult { Id = 1, UserId = 1, TopicId = 3, UserAnswer = [uaTrue]  },
            new LevelResult { Id = 2, UserId = 2, TopicId = 3, UserAnswer = [uaFalse] }
            );

        ctx.UserAnswers.AddRange(uaTrue, uaFalse);


        ctx.SaveChanges();
        return (ctx, conn);
    }

    [Fact]
    public async Task UpdateTopicMastery_True()
    {
        // Setup
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;
        int scopeId = 3;
        int userId = 1;
        bool mastered = true;

        var mock = new Mock<IAlgorithmRegistry<IUserMasteryAlgorithm>>();

        mock.Setup(f => f.Get(It.IsAny<string>()))
            .Returns(new SimpleUMA(context));

        var svc = new TopicService(context, Mock.Of<TopicHandler>(), Mock.Of<IAlgorithmRegistry<IUserProficiencyAlgorithm>>(), Mock.Of<IAlgorithmRegistry<INextTopicAlgorithm>>());

        // Act
        await svc.UpdateUserTopicMastery(scopeId, userId, mastered, Mock.Of<ILogger>());

        //Assert
        var testMastery = await (
            from up in context.UserScopeProgress
            where up.ScopeId == scopeId && up.UserId == userId
            select up.Mastered).SingleAsync();

        Assert.Equal(testMastery, mastered);
    }

    [Fact]
    public async Task UpdateTopicMastery_False()
    {
        // Setup
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;
        int scopeId = 3;
        int userId = 1;
        bool mastered = false;

        var svc = new TopicService(context, Mock.Of<TopicHandler>(), Mock.Of<IAlgorithmRegistry<IUserProficiencyAlgorithm>>(), Mock.Of<IAlgorithmRegistry<INextTopicAlgorithm>>());

        // Act
        await svc.UpdateUserTopicMastery(scopeId, userId, mastered, Mock.Of<ILogger>());

        //Assert
        var testMastery = await (
            from up in context.UserScopeProgress
            where up.ScopeId == scopeId && up.UserId == userId
            select up.Mastered).SingleAsync();

        Assert.False(testMastery);
    }

    [Fact]
    public async Task UpdateTopicMastery_Throws()
    {
        // Setup
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;
        int scopeId = 4;
        int userId = 1;
        bool mastered = false;

        var svc = new TopicService(context, Mock.Of<TopicHandler>(), Mock.Of<IAlgorithmRegistry<IUserProficiencyAlgorithm>>(), Mock.Of<IAlgorithmRegistry<INextTopicAlgorithm>>());

        // Act and Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => svc.UpdateUserTopicMastery(scopeId, userId, mastered, Mock.Of<ILogger>()));
        Assert.Equal("Topic with Id '4' not found", ex.Message);
    }

    [Fact]
    public async Task GetUserTopicInfo_CorrectInfo()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        int userId = 1;

        var mock = new Mock<IAlgorithmRegistry<IUserMasteryAlgorithm>>();

        mock.Setup(f => f.Get(It.IsAny<string>()))
            .Returns(new SimpleUMA(context));

        TopicService service = new(context, Mock.Of<TopicHandler>(), Mock.Of<IAlgorithmRegistry<IUserProficiencyAlgorithm>>(), Mock.Of<IAlgorithmRegistry<INextTopicAlgorithm>>());

        IEnumerable<UserTopicInfoDTO> topicInfos = await service.GetUserTopicInfo(userId);

        topicInfos.Should().BeEquivalentTo(
            [
                new UserTopicInfoDTO {
                    Available = true,
                    ScopeId = 2,
                    Proficiency = 1,
                    Mastered = true,
                    ScopeName = "mean",
                    AncestorId = 1,
                    AncestorName = "descriptive statistics"
                },
                new UserTopicInfoDTO {
                    Available = true,
                    ScopeId = 3,
                    Proficiency = 1,
                    Mastered = false,
                    ScopeName = "standard deviation",
                    AncestorId = 1,
                    AncestorName = "descriptive statistics"
                }
            ]
            );
    }

    [Fact]
    public async Task GetUserTopicInfo_CorrectInfoIncludesNotMappedTopics()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        int userId = 2;

        TopicService service = new TopicService(context, Mock.Of<TopicHandler>(), Mock.Of<IAlgorithmRegistry<IUserProficiencyAlgorithm>>(), Mock.Of<IAlgorithmRegistry<INextTopicAlgorithm>>());

        IEnumerable<UserTopicInfoDTO> topicInfos = await service.GetUserTopicInfo(userId);

        topicInfos.Should().BeEquivalentTo(
            [
                new UserTopicInfoDTO {
                    Available = true,
                    ScopeId = 2,
                    Proficiency = 1,
                    Mastered = false,
                    ScopeName = "mean",
                    AncestorId = 1,
                    AncestorName = "descriptive statistics"
                },
                new UserTopicInfoDTO {
                    Available = false,
                    ScopeId = 3,
                    Proficiency = 1,
                    Mastered = false,
                    ScopeName = "standard deviation",
                    AncestorId = 1,
                    AncestorName = "descriptive statistics"
                }
            ]
        );
    }

    [Fact]
    public async Task GetNextSuggestedTopic_CorrectTopicSuggested()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        int userId1 = 1,
            userId2 = 2;

        var mock = new Mock<IAlgorithmRegistry<INextTopicAlgorithm>>();

        mock.Setup(f => f.Get(It.IsAny<string>()))
            .Returns(new SimpleNTA(context));

        TopicService service = new TopicService(context, Mock.Of<TopicHandler>(), Mock.Of<IAlgorithmRegistry<IUserProficiencyAlgorithm>>(), mock.Object);

        ScopeDTO? nextSuggested1 = await service.GetNextSuggestedTopic(userId1);
        ScopeDTO? nextSuggested2 = await service.GetNextSuggestedTopic(userId2);

        Assert.NotNull(nextSuggested1);
        Assert.NotNull(nextSuggested2);

        Assert.Equivalent(new ScopeDTO
        {
            ScopeId = 3,
            ScopeName = "standard deviation",
            AncestorId = 1,
            AncestorName = "descriptive statistics"
        }, nextSuggested1);
        Assert.Equivalent(new ScopeDTO
        {
            ScopeId = 2,
            ScopeName = "mean",
            AncestorId = 1,
            AncestorName = "descriptive statistics"
        }, nextSuggested2);
    }

    [Fact]
    public async Task GetNextSuggestedTopic_NullWhenNoTopicsSuggested()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        int userId = 3;

        var mock = new Mock<IAlgorithmRegistry<INextTopicAlgorithm>>();

        mock.Setup(f => f.Get(It.IsAny<string>()))
            .Returns(new SimpleNTA(context));

        TopicService service = new TopicService(context, Mock.Of<TopicHandler>(), Mock.Of<IAlgorithmRegistry<IUserProficiencyAlgorithm>>(), mock.Object);

        ScopeDTO? test = await service.GetNextSuggestedTopic(userId);

        Assert.Null(test);
    }

}
