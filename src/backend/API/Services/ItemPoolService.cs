/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA;
using AA.NewItemsAlgorithm;
using API.Tools.EventQueue;
using Microsoft.EntityFrameworkCore;
using Models;

namespace API.Services;

public interface IItemPoolService
{
    Task GenerateItemsTillThreshold(int userId);
    Task QueueNewItemGeneration(int levelId);
    Task QueueNewItemGeneration(int topicId, int userId);
}

/// <summary>
/// Handles item pool management for topics.
/// Ensures that each topic has a minimum threshold of available items,
/// generates new items via the AI API when needed, and persists them to the database.
/// </summary>
public class ItemPoolService(
    AppDbContext context,
    IAIService aiService,
    IItemService itemService,
    IAlgorithmRegistry<INewItemsAlgorithm> niaRegistry,
    ILogger<ItemPoolService> logger,
    IEventQueue eventQueue
    ) : IItemPoolService
{
    // Many topics currently do not have more than 1 item and since item generation currently is not a scheduled action
    // putting this above 1 can lead to very long level generation times
    private const int Threshold = 1;


    /// <summary>
    /// Checks if the available items for a user is above the threshold for each topic.
    /// Generates new items via the AI API if the count falls below the threshold.
    /// </summary>
    /// <param name="userId">The ID of the user whose answered items are being checked.</param>
    public async Task GenerateItemsTillThreshold(int userId)
    {
        var query =
            from topic in context.Scopes
            join topicItem in context.ScopeItems on topic.Id equals topicItem.ScopeId into ti
            from topicItem in ti.DefaultIfEmpty()
            join item in itemService.NotRecentlySeenItems(userId)
                on topicItem.ItemId equals item.Id into items
            select new
            {
                Topic = topic,
                Count = items.Count()
            }
            into topicGroup
            let neededToGenerate = Threshold - topicGroup.Count
            where neededToGenerate > 0
            select new { topicGroup.Topic, NeededToGenerate = neededToGenerate };
        var resultList = await query.ToListAsync();

        foreach (var result in resultList)
        {
            await GenNewItem(result.Topic, result.NeededToGenerate);

            logger.LogInformation($"Generated {result.NeededToGenerate} items for topic {result.Topic.Id}");
        }
    }

    /// <summary>
    /// Generates a new item for the specified topic using the AI API.
    /// Adds the item to the database, updates answer identifiers to include the new item ID,
    /// and saves changes.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="amount">The number of items that should be generated</param>
    /// <exception cref="Exception"></exception>
    private async Task GenNewItem(Scope scope, int amount = 1)
    {
        // this takes a while so we can't put this in the transaction
        List<Item> newItems = await aiService.MakeQuestions(scope.Id, amount);
        foreach (Item newItem in newItems)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                newItem.ScopeItems.Add(new ScopeItem { ScopeId = scope.Id, ItemId = newItem.Id });
                context.Items.Add(newItem);

                // needed so we can assign an id to an item
                await context.SaveChangesAsync();

                foreach (var answer in newItem.Answers)
                {
                    // assign a new identifier
                    // honestly I have no clue why we do this or why we don't do this if section_ doesn't exist
                    var idx = answer.AnswerIdentifier.IndexOf("section_", StringComparison.Ordinal);
                    if (idx != -1)
                    {
                        var prefix = answer.AnswerIdentifier.Substring(0, idx + "section_".Length);
                        answer.AnswerIdentifier = $"{prefix}{newItem.Id}_item_1_num_RESPONSE_1_3075616";
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error trying to generate items for topic {scope.Id}", e);
            }
        }
    }

    /// <summary>
    /// Queues the generation of the number of items calculated by the <see cref="INewItemsAlgorithm"/> in the <see cref="IEventQueue"/>. 
    /// </summary>
    /// <param name="levelId">The levelId used for extracting the topic and user from a <see cref="LevelResult"/></param>
    public async Task QueueNewItemGeneration(int levelId)
    {
        var result = await (
            from lr in context.LevelResults
            where lr.Id == levelId
            select new { lr.TopicId, lr.UserId }
        ).SingleOrDefaultAsync() ?? throw new Exception($"No LevelResult with the specified id found");

        await QueueNewItemGeneration(result.TopicId, result.UserId);
    }

    /// <summary>
    /// Queues the generation of the number of items calculated by the <see cref="INewItemsAlgorithm"/> in the <see cref="IEventQueue"/>. 
    /// </summary>
    /// <param name="topicId">The topicId for which new items might need to be generated</param>
    /// <param name="userId">The Id of the user which requests the level, used for calculating the number of items needed </param>
    public async Task QueueNewItemGeneration(int topicId, int userId)
    {
        var algorithm = niaRegistry.Get("Simple NIA");
        NewItemsRecord newItems = await algorithm.NewItemBatch(topicId, userId);

        if (newItems is null || newItems.TotalNewItems == 0)
            return;

        await eventQueue.QueueAsync(new EventData(EventType.ItemGeneration, newItems), CancellationToken.None);
    }
}

