/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.UserMasteryAlgorithm;
using Models;
using Xunit;

namespace AATests.UserMasteryAlgorithm;

public class UrningsUMATests
{
    private readonly AppDbContext db;
    private readonly UrningsUMA algorithm;

    public UrningsUMATests()
    {
        db = SqliteInMemoryContextFactory.Create();
        algorithm = new UrningsUMA(db);
    }
    private async Task SetupDb(int greenBalls, int redBalls, decimal proficiency)
    {
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.UserScopeProgress.Add(new UserScopeProgress
        {
            UserId = 1,
            ScopeId = 1,
            GreenBalls = greenBalls,
            RedBalls = redBalls,
            Proficiency = proficiency
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CalculateMastery_WhenTopicNotFound_Throws()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => algorithm.CalculateMastery(1, 99, 1));
        Assert.Equal("Sequence contains no elements.", ex.Message);
    }

    [Fact]
    public async Task CalculateMastery_WhenUserTopicProgressNotFound_Throws()
    {
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => algorithm.CalculateMastery(1, 1, 1));
        Assert.Equal("Sequence contains no elements.", ex.Message);
    }

    [Fact]
    public async Task CalculateMastery_WhenUserUrnEmpty_Throws()
    {
        await SetupDb(0, 0, 0.5M);

        var ex = await Assert.ThrowsAsync<Exception>(() => algorithm.CalculateMastery(1, 1, 1));
        Assert.Equal("User 1's urn contains no balls", ex.Message);
    }

    // If this test breaks, its most likely because you changed the rating weights.
    [Theory]

    // Stress tests with set proficiency
    [InlineData(1, 0, 0.5, true)]
    [InlineData(1, 1, 0.5, false)]
    [InlineData(1000, 1, 0.5, true)]
    [InlineData(1, 1000, 0.5, false)]

    // Edge cases with set proficiency
    [InlineData(8, 1, 0.5, true)]
    [InlineData(7, 1, 0.5, true)]
    [InlineData(6, 1, 0.5, false)]
    [InlineData(5, 1, 0.5, false)]
    [InlineData(15, 2, 0.5, true)]
    [InlineData(14, 2, 0.5, true)]
    [InlineData(13, 2, 0.5, false)]
    [InlineData(12, 2, 0.5, false)]
    [InlineData(71, 10, 0.5, true)]
    [InlineData(70, 10, 0.5, true)]
    [InlineData(69, 10, 0.5, false)]
    [InlineData(68, 10, 0.5, false)]

    // Varying proficiency
    [InlineData(7, 2, 0.9, true)]
    [InlineData(9, 1, 0.1, false)]

    // Clamp boundary
    [InlineData(3, 10, 0.5, false)]
    [InlineData(4, 10, 0.5, false)]

    // Proficiency alone cannot reach threshold even at max
    [InlineData(0, 1, 1.0, false)]
    [InlineData(1, 0, 0.0, false)]
    public async Task CalculateMastery_TruePositiveOrNegative(int greenBalls, int redBalls, decimal proficiency, bool expected)
    {
        await SetupDb(greenBalls, redBalls, proficiency);

        bool actual = await algorithm.CalculateMastery(1, 1, 1);
        Assert.Equal(expected, actual);
    }
}