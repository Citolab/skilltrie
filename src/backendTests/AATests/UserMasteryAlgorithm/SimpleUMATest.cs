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

public class SimpleUMATests
{
    private readonly AppDbContext db;
    private readonly SimpleUMA algorithm;

    public SimpleUMATests()
    {
        db = SqliteInMemoryContextFactory.Create();
        algorithm = new SimpleUMA(db);
    }

    [Fact]
    public async Task CalculateMastery_WhenTopicNotFound_Throws()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => algorithm.CalculateMastery(1, 99, 1));
        Assert.Equal("Topic not found", ex.Message);
    }

    [Fact]
    public async Task CalculateMastery_WhenTestNotFound_Throws()
    {
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => algorithm.CalculateMastery(1, 1, 1));
        Assert.Equal("Sequence contains no elements.", ex.Message);
    }

    [Theory]
    // Stresstest
    [InlineData(1, 0, true)]
    [InlineData(1, 1, false)]
    [InlineData(1000, 1, true)]
    [InlineData(1, 1000, false)]

    // Edge cases
    [InlineData(5, 1, true)]
    [InlineData(4, 1, true)]
    [InlineData(3, 1, false)]
    [InlineData(2, 1, false)]

    [InlineData(9, 2, true)]
    [InlineData(8, 2, true)]
    [InlineData(7, 2, false)]
    [InlineData(6, 2, false)]

    [InlineData(41, 10, true)]
    [InlineData(40, 10, true)]
    [InlineData(39, 10, false)]
    [InlineData(38, 10, false)]
    public async Task CalculateMastery_TruePositiveOrNegative(int correct, int incorrect, bool expected)
    {
        db.Scopes.Add(new Scope { Id = 1, Name = "A" });
        db.Users.Add(new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", UserName = "test@test.com", DisplayName = "Test" });
        db.Items.Add(new Item { Id = 1, QuestionText = "hi", ResponseType = "conceptual"});

        List<UserAnswer> userAnswers = [];

        for (int j = 0; j < correct; j++)
            userAnswers.Add(new UserAnswer()
            {
                ItemId = 1,
                Correct = true
            });

        for (int j = 0; j < incorrect; j++)
            userAnswers.Add(new UserAnswer()
            {
                ItemId = 1,
                Correct = false
            });

        LevelResult testResult = new()
        {
            UserId = 1,
            UserAnswer = userAnswers
        };

        db.LevelResults.Add(testResult);
        foreach (UserAnswer ua in userAnswers)
            db.UserAnswers.Add(ua);

        await db.SaveChangesAsync();

        bool actual = await algorithm.CalculateMastery(1, 1, 1);
        Assert.Equal(expected, actual);
    }
}