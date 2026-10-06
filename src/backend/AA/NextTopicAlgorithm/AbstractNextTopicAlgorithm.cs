 /*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.ComponentModel.Design.Serialization;
using System.Data.Common;
using System.Reflection.PortableExecutable;
using Microsoft.EntityFrameworkCore;
using Models;

namespace AA.NextTopicAlgorithm;

public abstract class AbstractNextTopicAlgorithm(AppDbContext context) : INextTopicAlgorithm
{
    protected readonly AppDbContext _context = context;

    public abstract Task<Scope?> RecommendedTopic(int userId);

    /// <summary>
    /// Checks for a <see cref="User"/> which <see cref="Scope"/> topics are unlocked.
    /// This includes mastered topics.
    /// </summary>
    /// <param name="userId">The Id of the user.</param>
    public virtual async Task<List<Scope>> UnlockedTopics(int userId)
    {
        if (!context.Users.Any(u => u.Id == userId))
            throw new Exception($"User not found");

        var unlockedTopics = new List<Scope>();

        List<Scope> topics = await _context.Scopes
            .Where(t => _context.ScopeEdges
                .Any(td => td.ToScopeId == t.Id || td.FromScopeId == t.Id))
            .ToListAsync();

        foreach (var topic in topics)
        {
            if (await TopicIsUnlocked(userId, topic.Id))
                unlockedTopics.Add(topic);
        }

        return unlockedTopics;
    }


    /// <summary>
    /// Checks whether a specified <see cref="Scope"/> topic is unlocked for one <see cref="User"/>.
    /// This includes mastered topics.
    /// </summary>
    /// <param name="userId">The Id of the user.</param>
    /// <param name="topicId">The Id of the topic.</param>
    public virtual async Task<bool> TopicIsUnlocked(int userId, int topicId)
    {
        // Given a topicId, all ancestors need to be mastered to label it 'unlocked' per definition of the knowledge tree.
        if (!context.Scopes.Any(t => t.Id == topicId))
            throw new Exception($"Topic not found");
        if (!context.Users.Any(u => u.Id == userId))
            throw new Exception($"User not found");

        var ancestorIds = await (
            from se in context.ScopeEdges
            where se.ToScopeId == topicId
            select se.FromScopeId
        ).ToListAsync();

        if(ancestorIds.Count == 0)
            return true;

        var mastery = await (
            from up in context.UserScopeProgress
            where up.UserId == userId && ancestorIds.Contains(up.ScopeId)
            select up.Mastered
        ).ToListAsync();

        if(mastery.Count != ancestorIds.Count)
            throw new Exception("Not all ancestor topics have a proficiency entry for this user.");

        return mastery.All(x => x);
    }
}
 
internal static class PotentialTopicHelper
{
    /// <summary>
    /// Helper method for filtering from a list of topics the topics which are not mastered
    /// </summary>
    /// <param name="topics">The list of topics to filter from</param>
    /// <param name="userId">The user for which to check which topics are not mastered</param>
    /// <param name="context">The database context, needed as it is a helper method</param>
    public static List<Scope> NotMastered(this List<Scope> topics, int userId, AppDbContext context)
    {
        List<int> masteredTopicIds = 
            context.UserScopeProgress.Where(sp => sp.UserId == userId && sp.Mastered)
                .Select(sp => sp.ScopeId)
                .ToList();
        
        return topics
            .Where(t => !masteredTopicIds.Contains(t.Id))
            .ToList();
    }
}
