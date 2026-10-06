/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.EntityFrameworkCore;
using Models;

namespace AA.NextTopicAlgorithm;

/// <summary>
/// An implementation of the next topic algorithm that takes into account the proficiency of prerequisite topics.
/// </summary>
/// <param name="context">The database context</param>
[Algorithm("V1 NTA")]
public class V1NTA(AppDbContext context) : AbstractNextTopicAlgorithm(context)
{
    /// <summary>
    /// Return the unlocked topic with the highest weighted average proficiency of all its prerequisite topics for a <see cref="User"/>.
    /// </summary>
    /// <param name="userId">The Id of the user.</param>
    /// <returns>The unlocked topic with the highest weighted average proficiency. If there is a tie in proficiency, the topic with least prerequisites is returned.</returns>
    public override async Task<Scope?> RecommendedTopic(int userId)
    {
        if (!context.Users.Any(u => u.Id == userId))
            throw new Exception($"User not found");

        var unlocked = await UnlockedTopics(userId);

        if(!unlocked.Any())
            throw new Exception("No unlocked topics found for this user.");
        
        List<Scope> potentialTopics = unlocked.NotMastered(userId, context);

        if (potentialTopics.Count == 0)
            return null;

        List<(Scope, decimal, int)> RecommenderList = new();

        foreach (var potentialTopic in potentialTopics)
            RecommenderList.Add(await GetWeightedProficiency(userId, potentialTopic));

        RecommenderList = RecommenderList.OrderByDescending(x => x.Item2).ThenBy(x => x.Item3).ToList();

        return RecommenderList.First().Item1;
    }


    /// <summary>
    /// Calculate the weighted average proficiency of all prerequisite topics for a given topic and user.
    /// </summary>
    /// <param name="userId">The Id of the user.</param>
    /// <param name="topic">The topic for which to calculate weighted proficiency.</param>
    /// <returns>A tuple of the topic, its weighted proficiency, and the number of prerequisite topics.</returns>
    private async Task<(Scope, decimal, int)> GetWeightedProficiency(int userId, Scope topic)
    {
        List<int> prerequisites = await GetAllAncestorIds(topic.Id);

        var proficiencies = await (
            from up in context.UserScopeProgress
            where up.UserId == userId && prerequisites.Contains(up.ScopeId)
            select up.Proficiency
        ).ToListAsync();

        if(proficiencies.Count != prerequisites.Count)
            throw new Exception("Not all proficiencies could be found for topics!");

        decimal weightedProficiency = proficiencies.Sum() / prerequisites.Count;

        return (topic, weightedProficiency, prerequisites.Count);
    }

    /// <summary>
    /// Recursively get the Ids of all ancestor topics for a given topic. This is needed to calculate the weighted proficiency for a topic, which depends on all its ancestors per definition of the knowledge tree.
    /// </summary>
    /// <param name="topicId">The Id of the topic for which to find ancestors.</param>
    /// <returns>A list of ancestor topic IDs.</returns>
    private async Task<List<int>> GetAllAncestorIds(int topicId)
    {
        var ancestorIds = await(
            from se in context.ScopeEdges
            where topicId == se.ToScopeId
            select se.FromScopeId
        ).ToListAsync();

        if (ancestorIds.Count == 0)
            return new List<int>();

        var res = ancestorIds.ToList();

        foreach (var ancestor in ancestorIds)
        {
            var more = await GetAllAncestorIds(ancestor);
            res.AddRange(more);
        }
        return res.Distinct().ToList();
    }
}
