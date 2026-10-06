/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Globalization;
using System.Reflection;
using AA.NextTopicAlgorithm;
using Models;
using Xunit;

namespace AATests.NextTopicAlgorithm;

public class V1NTATests
{
    private readonly AppDbContext db;
    private readonly V1NTA algorithm;

    public V1NTATests()
    {
        db = SqliteInMemoryContextFactory.Create();
        algorithm = new V1NTA(db);
    }

    [Fact]
    public async Task RecommendedTopic_WhenUserNotFound_Throws()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => algorithm.RecommendedTopic(99));
        Assert.Equal("User not found", ex.Message);
    }

    [Fact]
    public async Task RecommendedTopic_WhenNoTopicsUnlocked_Throws()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        await db.SaveChangesAsync();
        var ex = await Assert.ThrowsAsync<Exception>(() => algorithm.RecommendedTopic(1));
        Assert.Equal("No unlocked topics found for this user.", ex.Message);
    }

    [Fact]
    public async Task RecommendedTopic_ReturnHighestProficiencyTopic()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        db.Scopes.Add(new Scope { Id = 2, Name = "B" });
        db.Scopes.Add(new Scope { Id = 3, Name = "C" });
        db.Scopes.Add(new Scope { Id = 4, Name = "D" });

        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 1, ToScopeId = 2 });
        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 3, ToScopeId = 4 });

        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 1, Proficiency = 1m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 2, Proficiency = 0m, Mastered = false });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 3, Proficiency = 2m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 4, Proficiency = 0m, Mastered = false });

        await db.SaveChangesAsync();

        var chosen = await algorithm.RecommendedTopic(1);
        Assert.Equal(4, chosen.Id);
    }

    [Fact]
    public async Task RecommendedTopic_ReturnsCorrectTopic()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        db.Scopes.Add(new Scope { Id = 2, Name = "B" });
        db.Scopes.Add(new Scope { Id = 3, Name = "C" });
        db.Scopes.Add(new Scope { Id = 4, Name = "D" });
        db.Scopes.Add(new Scope { Id = 5, Name = "E" });

        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 1, ToScopeId = 3 });
        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 2, ToScopeId = 3 });
        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 4, ToScopeId = 5 });


        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 1, Proficiency = 2m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 2, Proficiency = 1m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 3, Proficiency = 0m, Mastered = false });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 4, Proficiency = 1m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 5, Proficiency = 0m, Mastered = false });

        await db.SaveChangesAsync();

        var chosen = await algorithm.RecommendedTopic(1);
        Assert.Equal(3, chosen.Id);
    }

    [Fact]
    public async Task RecommendedTopic_ReturnCorrectTopicWhenTieInProficiency()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        db.Scopes.Add(new Scope { Id = 2, Name = "B" });
        db.Scopes.Add(new Scope { Id = 3, Name = "C" });
        db.Scopes.Add(new Scope { Id = 4, Name = "D" });
        db.Scopes.Add(new Scope { Id = 5, Name = "E" });

        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 1, ToScopeId = 3 });
        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 2, ToScopeId = 3 });
        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 4, ToScopeId = 5 });


        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 1, Proficiency = 1m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 2, Proficiency = 1m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 3, Proficiency = 0m, Mastered = false });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 4, Proficiency = 2m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 5, Proficiency = 0m, Mastered = false });

        await db.SaveChangesAsync();

        var chosen = await algorithm.RecommendedTopic(1);
        Assert.Equal(5, chosen.Id);
    }


    //By checking if the first element is returned with equal proficiency and equal count of prerequisites, we can also determine if all ancestors are correctly found and included in the calculation
    [Fact]
    public async Task GetAllAncestorIds_ReturnsAllAncestors()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        db.Scopes.Add(new Scope { Id = 2, Name = "B" });
        db.Scopes.Add(new Scope { Id = 3, Name = "C" });
        db.Scopes.Add(new Scope { Id = 4, Name = "D" });
        db.Scopes.Add(new Scope { Id = 5, Name = "E" });

        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 1, ToScopeId = 2 });
        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 2, ToScopeId = 3 });
        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 1, ToScopeId = 4 });
        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 4, ToScopeId = 5 });

        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 1, Proficiency = 1m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 2, Proficiency = 1m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 3, Proficiency = 0m, Mastered = false });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 4, Proficiency = 1m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 5, Proficiency = 0m, Mastered = false });

        await db.SaveChangesAsync();

        var result = await algorithm.RecommendedTopic(1);

        Assert.Equal(3, result.Id);
    }

}
