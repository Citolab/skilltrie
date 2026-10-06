/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace AA.NextTopicAlgorithm;

/// <summary>
/// The simplest implementation of the next topic algorithm.
/// </summary>
/// <param name="context">The database context</param>
[Algorithm("Simple NTA")]
public class SimpleNTA(AppDbContext context) : AbstractNextTopicAlgorithm(context)
{
    /// <summary>
    /// Returns the first unlocked topic for a <see cref="User"/>.
    /// </summary>
    /// <param name="userId">The Id of the user.</param>
    public override async Task<Scope?> RecommendedTopic(int userId)
    {
        List<Scope> unlockedTopics = await UnlockedTopics(userId);
        if(unlockedTopics.Count == 0)
            throw new Exception("No unlocked topics found for this user.");

        List<Scope> potentialTopics = unlockedTopics.NotMastered(userId, context);
        
        return potentialTopics.Count >= 1 ? potentialTopics.First() : null;
    }
}
