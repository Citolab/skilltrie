/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using Microsoft.EntityFrameworkCore;

namespace AA.NewItemsAlgorithm;

/// <summary>
/// The simplest implementation of the new items algorithm.
/// </summary>
/// <param name="context">The database context</param>
[Algorithm("Simple NIA")]
public class SimpleNIA(AppDbContext context) : INewItemsAlgorithm
{
    public async Task<NewItemsRecord> NewItemBatch(int topicId, int userId)
    {
        if (!context.Users.Any(u => u.Id == userId))
            throw new Exception($"User not found");

        if (!context.Scopes.Any(t => t.Id == topicId))
            throw new Exception("Topic not found");

        Setting activeSetting = await GetActiveSetting();
        int minItemsThreshold = activeSetting.LevelSize;
        DateTime redoTime = DateTime.UtcNow - activeSetting.TimeBeforeRedo;

        // Number of active items for the specified topic
        int topicActiveItems = await (
            from items in context.Items
            join topitems in context.ScopeItems
                on items.Id equals topitems.ItemId
            where topitems.ScopeId == topicId
                && items.Active
            select items.Id
            ).CountAsync();

        // Number of unique items the specified user has answered for the specified topic, that the user can't make again until the redoTime has passed
        int userUniqueAnswerCount = await (
            from userAnswer in context.UserAnswers
            join levelres in context.LevelResults
                on userAnswer.LevelResultId equals levelres.Id
            where levelres.CreatedAt >= redoTime
                && levelres.TopicId == topicId
                && levelres.UserId == userId
            select userAnswer.ItemId
        ).Distinct().CountAsync();

        int itemsAvailable = topicActiveItems - userUniqueAnswerCount;
        if (itemsAvailable >= minItemsThreshold)
            return new NewItemsRecord() { TopicId = topicId, UserId = userId, TotalNewItems = 0 };

        int itemsToGenerate = minItemsThreshold - itemsAvailable;

        return new NewItemsRecord() { TopicId = topicId, UserId = userId, TotalNewItems = itemsToGenerate, MultipleChoice = itemsToGenerate };
    }

    /// <summary>
    /// Returns the <see cref="Setting"/> that is currently activated
    /// </summary>
    async Task<Setting> GetActiveSetting()
    {
        try
        {
            Setting activeSetting = await (
                from settings in context.Settings
                orderby settings.LastActive descending
                select settings
            ).FirstAsync();
            return activeSetting;
        }
        catch (Exception)
        {
            throw new Exception("No active setting found");
        }
    }
}