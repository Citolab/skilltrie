/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Reflection;
using AA.NextTopicAlgorithm;
using Models;
using Xunit;

namespace AATests.NextTopicAlgorithm;

public class SimpleNTATests
{
    private readonly AppDbContext db;
    private readonly SimpleNTA algorithm;

    public SimpleNTATests()
    {
        db = SqliteInMemoryContextFactory.Create();
        algorithm = new SimpleNTA(db);
    }

    [Fact]
    public async Task RecommendedTopic_WhenUserNotFound_Throws()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => algorithm.RecommendedTopic(99));
        Assert.Equal("User not found", ex.Message);
    }

    [Fact]
    public async Task RecommendedTopic_ReturnsFirstAvailableTopic(){

        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        db.Scopes.Add(new Scope { Id = 2, Name = "B" });
        db.Scopes.Add(new Scope { Id = 3, Name = "C" });

        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 1, ToScopeId = 2 });

        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 1, Proficiency = 0m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 2, Proficiency = 0m, Mastered = false });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 3, Proficiency = 0m, Mastered = false });

        await db.SaveChangesAsync();

        var recommended = await algorithm.RecommendedTopic(1);
        Assert.Equal(2, recommended.Id);
    }

}
