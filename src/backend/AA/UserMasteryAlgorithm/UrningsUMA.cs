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
/// A v2 user mastery algorithm, based on Urnings rating and Proficiency
/// </summary>
/// <param name="context"></param>
[Algorithm("Urnings UMA")]
public class UrningsUMA(AppDbContext context) : IUserMasteryAlgorithm
{
    /// Weights and thresholds for the algorithm, which can be tweaked to adjust the balance between Urnings rating and proficiency.

    /*
    The default urnings rating clamp is 0.3 because anything below it means the user is getting a lot of questions wrong,
    and we dont want to punish users too harshly for that. Currently the clamp does not create any situations
    where a user does master a topic while without the clamp they wouldn't have, but under different parameters
    it might play a big role. (In the future)
    */
    readonly decimal clampUrningsRating = 0.3M;

    /*
    User needs a weighted average of the two ratings at least as high as masteryThreshold to master a topic.
    Currently set to 0.75, a bit lower than you might expect because Urnings rating takes relatively long to increase.
    */
    readonly decimal masteryThreshold = 0.75M;

    /*
    We set the urnings rating to weigh a bit heavier, meaning long-term results are more important
    than short-term results. This is of course subject to change.
    */
    readonly int urningsRatingWeight = 2;
    readonly int proficiencyWeight = 1;

    /// <summary>
    /// Returns the user mastery of one <see cref="User"/> for one <see cref="Scope"/>.
    /// </summary>
    /// <param name="userId">The Id of the user.</param>
    /// <param name="topicId">The Id of the topic.</param>
    /// <param name="testId">The Id of the latest finished test.</param>
    public async Task<bool> CalculateMastery(int userId, int topicId, int testId)
    {
        var utp = await (
            from tp in context.UserScopeProgress
            where tp.UserId == userId && tp.ScopeId == topicId
            select tp
            ).SingleAsync();

        if (utp.GreenBalls + utp.RedBalls == 0)
            throw new Exception($"User {userId}'s urn contains no balls");

        /*
        We clamp Urnings rating to the value of clampUrningsRating to prevent users from being too harshly punished 
        for getting a few questions wrong, especially in the beginning 
        when they are still learning the topic.
        */

        // [clampUrningsRating - 1]
        var urningsRating = Math.Max(clampUrningsRating, (decimal)utp.GreenBalls / (utp.GreenBalls + utp.RedBalls));

        // [0 - 1]
        var proficiency = utp.Proficiency;

        /*
        We check the weighted average of the two ratings against the mastery threshold.
        This is to balance out the long-term knowledge (from Urnings)
        and the short-term knowledge (from proficiency, which is updated after each test) of the user.
        */
        decimal weightedRating = (urningsRating * urningsRatingWeight + proficiency * proficiencyWeight) 
            / (urningsRatingWeight + proficiencyWeight);

        return weightedRating >= masteryThreshold;
    }
}