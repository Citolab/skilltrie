/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.DefaultProficiencyAlgorithm;
using Models;
using Xunit;

namespace AATests.DefaultProficiencyAlgorithm;

public class SimpleDPATests
{
    private readonly AppDbContext db;
    private readonly SimpleDPA algorithm;

    public SimpleDPATests()
    {
        db = SqliteInMemoryContextFactory.Create();
        algorithm = new SimpleDPA(db);
    }

    [Fact]
    public async Task CreateDefaultProficiencies_WhenUserNotFound_Throws()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => algorithm.CreateDefaultProficiencies(1));
        Assert.Equal("User with Id '1' not found.", ex.Message);
    }

    [Fact]
    public async Task CreateDefaultProficiencies_CreatesOneProfilePerTopic()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.AddRange(
            new Scope { Id = 1, Name = "A" },
            new Scope { Id = 2, Name = "B" },
            new Scope { Id = 3, Name = "C" }
        );
        await db.SaveChangesAsync();

        await algorithm.CreateDefaultProficiencies(1);

        var proficiencies = db.UserScopeProgress.ToList();
        Assert.Equal(3, proficiencies.Count);
        Assert.All(proficiencies, p => Assert.Equal(1, p.UserId));
    }

    [Fact]
    public async Task CreateDefaultProficiencies_ProficiencyValuesAreWithinRange()
    {
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Scopes.AddRange(
            new Scope { Id = 1, Name = "A" },
            new Scope { Id = 2, Name = "B" }
        );
        await db.SaveChangesAsync();

        await algorithm.CreateDefaultProficiencies(1);

        var proficiencies = db.UserScopeProgress.ToList();
        Assert.All(proficiencies, p => Assert.InRange(p.Proficiency, 0.5m, 1.0m));
    }
}