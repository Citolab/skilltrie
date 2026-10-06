/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA;
using AA.NextTopicAlgorithm;
using AA.UserProficiencyAlgorithm;
using API.Controllers.DTOs;
using API.Handlers;
using Microsoft.EntityFrameworkCore;
using Models;

namespace API.Services;


public interface ITopicService
{
    Task VerifyTopicExists(int id);
    Task<Item> UpdateTopicsForItem(Item item, ICollection<int> topicItemIds);
    Task<IEnumerable<ScopeDTO>> GetUnlockedTopics(int userId);
    Task<bool> IsUnlockedTopic(int scopeId, IEnumerable<UserTopicInfoDTO> userTopicInfos);
    Task<IEnumerable<ScopeDTO>> GetAllTopics();
    Task UpdateUserProficiency(int topicId, int userId);
    Task DefaultUserProficiencies(int userId);
    Task UpdateUserTopicMastery(int topicId, int userId, bool mastered, ILogger logger);
    Task DefaultTopicMastery(int userId);
    Task<IEnumerable<UserTopicInfoDTO>> GetUserTopicInfo(int userId);
    Task<ScopeDTO?> GetNextSuggestedTopic(int userId);

    Task<IEnumerable<ScopeDependencyDTO>> GetTopicDependencies();
}
/// <summary>
/// The class that will execute the interactions with the database for the <see cref="AppDbContext.Scopes"/>, <see cref="AppDbContext.ScopeMemberships"/> and <see cref="AppDbContext.UserScopeProgress"/> tables.
/// </summary>
/// <param name="context">The database context</param>
/// <param name="topicHandler">The topic handler</param>
public class TopicService(
        AppDbContext context,
        TopicHandler topicHandler,
        IAlgorithmRegistry<IUserProficiencyAlgorithm> upaRegistry,
        IAlgorithmRegistry<INextTopicAlgorithm> ntaRegistry
    ) : ITopicService
{
    public async Task VerifyTopicExists(int id)
    {
        bool topicExists = await context.Scopes.AnyAsync(i => i.Id == id);
        if (!topicExists)
            throw new Exception("Topic not found");
    }

    /// <summary>
    /// Sets a new <c>ICollection</c> for the <see cref="Item.Scopes"/> with the given new <c>ICollection</c> of <see cref="Scope"/> instances.
    /// </summary>
    /// <param name="item">The item to edit topics for.</param>
    /// <param name="topicItemIds"></param>
    /// <returns>The edited <see cref="Item"/> with updated <see cref="Item.Scopes"/></returns>
    public async Task<Item> UpdateTopicsForItem(Item item, ICollection<int> topicItemIds)
    {
        item.Scopes = await (
            from scopeItem in context.ScopeItems
            where topicItemIds.Contains(scopeItem.ScopeId)
            select scopeItem.Scope
        ).ToListAsync();

        return item;
    }

    /// <summary>
    /// Gets the list of all unlocked topics for a given user.
    /// </summary>
    /// <param name="userId">The Id of the user the <see cref="UserScopeProgress"/> needs to be sufficient for.</param>
    /// <returns></returns>
    public async Task<IEnumerable<ScopeDTO>> GetUnlockedTopics(int userId)
    {
        return (
            from uti in await GetUserTopicInfo(userId)
            where uti.Available
            select uti as ScopeDTO
            );
    }

    /// <summary>
    /// Returns whether the given scope is unlocked depending on the userTopicInfos
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="userTopicInfos"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<bool> IsUnlockedTopic(int scopeId, IEnumerable<UserTopicInfoDTO> userTopicInfos)
    {
        if (!context.Scopes.Any(t => t.Id == scopeId))
            throw new Exception($"Scope.Id='{scopeId}' does not exist");

        var ancestorIds = await (
            from se in context.ScopeEdges
            where se.ToScopeId == scopeId
            select se.FromScopeId
        ).ToListAsync();

        if (ancestorIds.Count == 0)
            return true;

        var mastery = (
            from up in userTopicInfos
            where ancestorIds.Contains(up.ScopeId)
            select up.Mastered
        ).ToList();

        return mastery.All(x => x == true);
    }

    /// <summary>
    /// Gets all topics of a set subject of the <see cref="Scope"/> tree based on the <see cref="ScopeMembership"/> table.
    /// </summary>
    /// <returns>
    /// A list <see cref="ScopeDTO"/> dto with Id being the Id of the <see cref="Scope"/> and the corresponding name.
    /// </returns>
    public async Task<IEnumerable<ScopeDTO>> GetAllTopics()
    {
        // We currently assume that 185 is the Id of the Scope for
        // which Scope.ScopeType=ScopeType.Subject and is the subject
        // we want to plot the tree for. The Id 185 corresponds with statistics
        int subjectId = 185;

        Scope subject = (await context.Scopes.FindAsync(subjectId))!;
        List<ScopeDTO> descendants = new();
        
        AddAllDescendantTopics(subject, descendants);

        return descendants;
    }
    
    /// <summary>
    /// Helper function that adds all topic descendants of a given <see cref="Scope"/> based on the <see cref="ScopeMembership"/> table to the given list
    /// </summary>
    /// <param name="ancestor"></param>
    /// <param name="descendants"></param>
    /// <returns></returns>
    private void AddAllDescendantTopics(Scope ancestor, List<ScopeDTO> descendants)
    {
        foreach (Scope descendant in GetDescendants(context, ancestor).ToList())
        {
            if (descendant.Type == ScopeType.Topic)
                descendants.Add(new ScopeDTO
                {
                    ScopeId = descendant.Id,
                    ScopeName = descendant.Name,
                    AncestorId = ancestor.Id,
                    AncestorName = ancestor.Name
                });

            AddAllDescendantTopics(descendant, descendants);
        }
    }

    /// <summary>
    /// Helper function to easily navigate through ScopeMembership to get ancestors
    /// </summary>
    /// <param name="context"></param>
    /// <param name="descendant"></param>
    /// <returns></returns>
    private IQueryable<Scope> GetAncestors(AppDbContext context, Scope descendant)
    {
        return (
            from se in context.ScopeMemberships
            where se.DescendantId == descendant.Id
            select se.Ancestor
            );
    }

    /// <summary>
    /// Helper function to easily navigate through ScopeMembership to get descendants
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ancestor"></param>
    /// <returns></returns>
    private IQueryable<Scope> GetDescendants(AppDbContext context, Scope ancestor)
    {
        return (
            from se in context.ScopeMemberships
            where se.AncestorId == ancestor.Id
            select se.Descendant
            );
    }

    /// <summary>
    /// Helper function to easily navigate through ScopeEdge to get scopes to the left in the tree
    /// </summary>
    /// <param name="context"></param>
    /// <param name="toScope"></param>
    /// <returns></returns>
    private IQueryable<Scope> GetFromScopes(AppDbContext context, Scope toScope)
    {
        return (
            from se in context.ScopeEdges
            where se.ToScopeId == toScope.Id
            select se.FromScope
            );
    }

    /// <summary>
    /// Helper function to easily navigate through ScopeEdge to get scopes to the right in the tree
    /// </summary>
    /// <param name="context"></param>
    /// <param name="fromScope"></param>
    /// <returns></returns>
    private IQueryable<Scope> GetToScopes(AppDbContext context, Scope fromScope)
    {
        return (
            from se in context.ScopeEdges
            where se.FromScopeId == fromScope.Id
            select se.ToScope
            );
    }

    /// <summary>
    /// Will query a list of <see cref="UserTopicInfoDTO"/> and will actually fill the required data
    /// </summary>
    /// <param name="userId">The id of the user to return the topic information for</param>
    /// <returns>A list of <see cref="UserTopicInfoDTO"/> which contains the user-specific information for a topic</returns>
    public async Task<IEnumerable<UserTopicInfoDTO>> GetUserTopicInfo(int userId)
    {
        IEnumerable<ScopeDTO> topics = await GetAllTopics();
        IEnumerable<UserScopeProgress> userScopeProgresses = context.UserScopeProgress
            .Where(usp => usp.UserId == userId);
        List<UserTopicInfoDTO> userTopicInfos = topics.Select(topic =>
        {
            UserScopeProgress userScopeProgress = userScopeProgresses.First(usp => usp.ScopeId == topic.ScopeId);

            return new UserTopicInfoDTO
            {
                ScopeId = topic.ScopeId,
                ScopeName = topic.ScopeName,
                AncestorId = topic.AncestorId,
                AncestorName = topic.AncestorName,
                Proficiency = userScopeProgress.Proficiency,
                Mastered = userScopeProgress.Mastered
            };
        }).ToList();

        foreach (var userTopicInfo in userTopicInfos)
            userTopicInfo.Available = await IsUnlockedTopic(userTopicInfo.ScopeId, userTopicInfos);

        return userTopicInfos;
    }

    /// <summary>
    /// Returns the next suggested topic for a specific user
    /// </summary>
    /// <param name="userId"> The user for which a topic needs to be suggested </param>
    public async Task<ScopeDTO?> GetNextSuggestedTopic(int userId)
    {
        var algorithm = ntaRegistry.Get("Simple NTA");
        Scope? recommendedTopic = await algorithm.RecommendedTopic(userId);
        if (recommendedTopic == null) return null;

        IEnumerable<ScopeDTO> topics = await GetAllTopics();

        ScopeDTO? topic = topics.FirstOrDefault(topic => topic?.ScopeId == recommendedTopic.Id, null);

        if (topic == null)
            throw new Exception("No topic could be suggested for the user");

        return topic;
    }

    /// <summary>
    /// Returns all the topic dependencies in the form of an edge list containing the from and to (topic) of the edge
    /// </summary>
    public async Task<IEnumerable<ScopeDependencyDTO>> GetTopicDependencies()
    {
        return await context.ScopeEdges
            .Select(se => new ScopeDependencyDTO {From = se.FromScopeId, To = se.ToScopeId, Weight = se.Weight})
            .ToListAsync();
    }


    /// <summary>
    /// Updates the <see cref="UserScopeProgress"/> of one <see cref="User"/> for one <see cref="Scope"/>.
    /// </summary>
    /// <param name="topicId">The Id of the <see cref="Scope"/> to update the proficiency for.</param>
    /// <param name="userId">The Id of the <see cref="User"/> to update the proficiency for.</param>
    public async Task UpdateUserProficiency(int topicId, int userId)
    {
        var algorithm = upaRegistry.Get("Simple UPA");
        await algorithm.UpdateProficiency(userId, topicId);
    }

    /// <summary>
    /// Sets default values for <see cref="UserScopeProgress"/> for the specified user.
    /// </summary>
    /// <param name="userId">The Id of the user for which to set default proficiencies.</param>
    public async Task DefaultUserProficiencies(int userId)
    {
        if (!await context.Users.AnyAsync(u => u.Id == userId))
            throw new Exception($"User with Id '{userId}' not found.");

        var topicIds = context.Scopes.Select(t => t.Id).ToArray();

        List<UserScopeProgress> proficiencies = topicHandler.GetDefaultProficiencies(userId, topicIds);

        context.UserScopeProgress.AddRange(proficiencies);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates the mastery of one <see cref="User"/> for one <see cref="Scope"/>.
    /// </summary>
    /// <param name="topicId">The Id of the <see cref="Scope"/> to update the mastery for.</param>
    /// <param name="userId">The Id of the <see cref="User"/> to update the mastery for.</param>
    /// <param name="mastered">The mastery value to set.</param>
    public async Task UpdateUserTopicMastery(int topicId, int userId, bool mastered, ILogger logger)
    {
        if (!context.Scopes.Any(t => t.Id == topicId))
            throw new Exception($"Topic with Id '{topicId}' not found");

        UserScopeProgress mastery = await (
            from up in context.UserScopeProgress
            where up.UserId == userId && up.ScopeId == topicId
            select up
        ).SingleAsync();

        mastery.Mastered = mastered;

        context.UserScopeProgress.Update(mastery);
        await context.SaveChangesAsync();

        logger.LogInformation($"User {userId} has mastered topic {topicId}");
    }

    /// <summary>
    /// Sets the default mastered topics for the user by selecting 10 topics and setting them to mastered.
    /// </summary>
    /// <param name="userId">The Id of the user to set the default values for.</param>
    /// <returns></returns>
    public async Task DefaultTopicMastery(int userId)
    {
        if (!await context.Users.AnyAsync(u => u.Id == userId))
            throw new Exception($"User with Id '{userId}' not found.");

        Random rnd = new Random();
        List<UserScopeProgress> usps = await context.UserScopeProgress
            .Where(up => up.UserId == userId)
            .OrderBy(x => Guid.NewGuid())
            .Take(10)
            .ToListAsync();

        foreach (UserScopeProgress usp in usps)
            usp.Mastered = true;

        await context.SaveChangesAsync();
    }
}
