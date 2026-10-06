/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.UserProficiencyAlgorithm;
using Models;
using Xunit;

namespace AATests.UserProficiencyAlgorithm;

public class SimpleUPATests
{
    private readonly AppDbContext db;
    private readonly simpleUPA algorithm;

    public SimpleUPATests()
    {
        db = SqliteInMemoryContextFactory.Create();
        algorithm = new simpleUPA(db);
    }

    [Fact]
    public async Task UpdateProficiency_WhenTopicNotFound_Throws()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => algorithm.UpdateProficiency(1, 99));
        Assert.Equal("Topic not found", ex.Message);
    }

    [Fact]
    public async Task UpdateProficiency_IncreaseProficiencyByGain()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 1, Proficiency = 0.5m });
        await db.SaveChangesAsync();

        await algorithm.UpdateProficiency(1, 1);

        var updated = db.UserScopeProgress.Single();
        Assert.Equal(0.6m, updated.Proficiency);
    }

    [Fact]
    public async Task UpdateProficiency_ClampsAtOne()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 1, Proficiency = 0.95m });
        await db.SaveChangesAsync();

        await algorithm.UpdateProficiency(1, 1);

        var updated = db.UserScopeProgress.Single();
        Assert.Equal(1.0m, updated.Proficiency);
    }

    [Fact]
    public void CalculateNewProficiency_AddsGain()
    {
        var result = algorithm.CalculateNewProficiency(0.5m);
        Assert.Equal(0.6m, result);
    }

    [Fact]
    public void CalculateNewProficiency_ClampsAtOne()
    {
        var result = algorithm.CalculateNewProficiency(1.0m);
        Assert.Equal(1.0m, result);
    }

    [Fact]
    public void CalculateNewProficiency_ClampsAtZero()
    {
        var result = algorithm.CalculateNewProficiency(-0.5m);
        Assert.Equal(0.0m, result);
    }
}