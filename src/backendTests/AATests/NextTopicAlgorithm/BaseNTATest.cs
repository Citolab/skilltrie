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

public class BaseNTATests
{
    private readonly AppDbContext db;
    private readonly SimpleNTA algorithm;

    public BaseNTATests()
    {
        db = SqliteInMemoryContextFactory.Create();
        algorithm = new SimpleNTA(db); //Any algorithm can be used here, as these tests only test the base functionality
    }

    [Fact]
    public async Task UnlockedTopics_WhenUserNotFound_Throws()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => algorithm.UnlockedTopics(99));
        Assert.Equal("User not found", ex.Message);
    }
    [Fact]
    public async Task IsUnlocked_WhenTopicNotFound_Throws()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        await db.SaveChangesAsync();
        var ex = await Assert.ThrowsAsync<Exception>(() => algorithm.TopicIsUnlocked(1, 99));
        Assert.Equal("Topic not found", ex.Message);
    }
    [Fact]
    public async Task IsUnlocked_WhenUserNotFound_Throws()
    {
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        await db.SaveChangesAsync();
        var ex = await Assert.ThrowsAsync<Exception>(() => algorithm.TopicIsUnlocked(99, 1));
        Assert.Equal("User not found", ex.Message);
    }

    [Fact]
    public async Task UnlockedTopics_ReturnsCorrectTopics(){

        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        db.Scopes.Add(new Scope { Id = 2, Name = "B" });
        db.Scopes.Add(new Scope { Id = 3, Name = "C" });

        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 1, ToScopeId = 2 });
        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 1, ToScopeId = 3 });


        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 1, Proficiency = 0m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 2, Proficiency = 0m, Mastered = false });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 3, Proficiency = 0m, Mastered = false });

        await db.SaveChangesAsync();

        var unlocked = await algorithm.UnlockedTopics(1);
        Assert.Contains(unlocked, t => t.Id == 1);
        Assert.Contains(unlocked, t => t.Id == 2);
        Assert.Contains(unlocked, t => t.Id == 3);
    }

    [Fact]
    public async Task IsUnlocked_ReturnsCorrectDependentTopics(){
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        db.Scopes.Add(new Scope { Id = 2, Name = "B" });
        db.Scopes.Add(new Scope { Id = 3, Name = "C" });
        db.Scopes.Add(new Scope { Id = 4, Name = "D" });


        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 1, ToScopeId = 3 });
        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 2, ToScopeId = 3 });
        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 1, ToScopeId = 4 });
        db.ScopeEdges.Add(new ScopeEdge { FromScopeId = 3, ToScopeId = 4 });

        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 1, Proficiency = 0m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 2, Proficiency = 0m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 3, Proficiency = 0m, Mastered = false });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 4, Proficiency = 0m, Mastered = false });

        await db.SaveChangesAsync();

        var unlocked = await algorithm.TopicIsUnlocked(1, 3);
        var unlocked2 = await algorithm.TopicIsUnlocked(1, 4);
        Assert.True(unlocked);
        Assert.False(unlocked2);
    }

    [Fact]
    public async Task IsUnlocked_ReturnsCorrectSingleTopics(){
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        db.Scopes.Add(new Scope { Id = 2, Name = "B" });

        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 1, Proficiency = 0m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 2, Proficiency = 0m, Mastered = false });

        await db.SaveChangesAsync();

        var unlocked = await algorithm.TopicIsUnlocked(1, 1);
        var unlocked2 = await algorithm.TopicIsUnlocked(1, 2);
        Assert.True(unlocked);
        Assert.True(unlocked2);
    }

    [Fact]
    public async Task IsUnlocked_ReturnsCorrectComplexTopics(){
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

        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 1, Proficiency = 0m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 2, Proficiency = 0m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 3, Proficiency = 0m, Mastered = false });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 4, Proficiency = 0m, Mastered = true });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 5, Proficiency = 0m, Mastered = false });

        await db.SaveChangesAsync();

        var unlocked = await algorithm.UnlockedTopics(1);
        Assert.Contains(unlocked, t => t.Id == 1);
        Assert.Contains(unlocked, t => t.Id == 2);
        Assert.Contains(unlocked, t => t.Id == 3);
        Assert.Contains(unlocked, t => t.Id == 4);
        Assert.Contains(unlocked, t => t.Id == 5);
    }
}
