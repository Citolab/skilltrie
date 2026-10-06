/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.EntityFrameworkCore;
using Models;

namespace AA.UserMasteryAlgorithm;

/// <summary>
/// The simplest implementation of the user mastery algorithm
/// </summary>
/// <param name="context"></param>
[Algorithm("Simple UMA")]
public class SimpleUMA(AppDbContext context) : IUserMasteryAlgorithm
{
    /// <summary>
    /// Returns the user mastery of one <see cref="User"/> for one <see cref="Scope"/>.
    /// </summary>
    /// <param name="userId">The Id of the user.</param>
    /// <param name="topicId">The Id of the topic.</param>
    /// <param name="testId">The Id of the latest finished test.</param>
    public async Task<bool> CalculateMastery(int userId, int topicId, int testId)
    {
        // Assumes that the test from testId is for the specified topic from topicId!
        // topicId technically not used in this version; may be necessary later.

        // Temporary fraction
        double masteryThreshold = 0.8;

        if (!context.Scopes.Any(t => t.Id == topicId))
            throw new Exception($"Topic not found");

        var lastTestResult = await (
            from lr in context.LevelResults
            where lr.UserId == userId && lr.Id == testId
            select lr.UserAnswer
        ).SingleAsync();

        int correct = lastTestResult.Sum(ua => ua.Correct ? 1 : 0);
        bool mastered = correct >= lastTestResult.Count * masteryThreshold;

        return mastered;
    }
}