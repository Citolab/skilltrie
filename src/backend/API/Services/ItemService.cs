/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA;
using AA.NewItemsAlgorithm;
using API.Controllers.DTOs;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace API.Services;

public interface IItemService
{
    Task<Item> VerifyItemExists(int id);
    Task<Item> GetItem(int itemId, bool withAnswers = false, bool withReports = false);
    Task<Item[]> GetItems(int offset, int range, int MaxQuestionBodyLength, string sortColumn, string sortOrder);
    Task<Item[]> GetItemsForAssessment(int[] itemIds);
    IQueryable<Item> NotRecentlySeenItems(int userId);
    Task<SmallItemDto[]> GetItemByAnswertext(string searchString, int MaxQuestionBodyLength);
    Task<Item> UpdateItem(Item UpdatedItem);
    Task<Item> ActivateItem(int Id);
    Task<Item> DeactivateItem(int Id);
    Task<string> CorrectAnswer(int itemId);
    Task ResolveReports(int Id);
}
public class ItemService(AppDbContext context, ITopicService topicService) : IItemService
{
    private const int DaysTillRedo = 10;


    public async Task<Item> VerifyItemExists(int id)
    {
        Item item;
        try
        {
            item = await context.Items.FirstAsync(i => i.Id == id);
        }
        catch (InvalidOperationException)
        {
            throw new Exception("Item not found");
        }

        return item;
    }

    /// <summary>
    /// Get an Item from the database by Id
    /// </summary>
    /// <param name="itemId">ItemID to be queried</param>
    /// <param name="withAnswers">Bool to decide if the item returned should contain the answer. Defaulted to no</param>
    /// <param name="withReports">Bool to decide if the item returned should contain the reports. Defaulted to no</param>
    /// <returns>An Item</returns>
    /// <exception cref="Exception">The item was not found in the database</exception>
    public async Task<Item> GetItem(int itemId, bool withAnswers = false, bool withReports = false)
    {
        IQueryable<Item> itemQuery = context.Items;

        if (withAnswers)
            itemQuery = itemQuery.Include(i => i.Answers);

        if (withReports)
            itemQuery = itemQuery.Include(i => i.Reports);

        var item = await itemQuery.FirstOrDefaultAsync(i => i.Id == itemId)
            ?? throw new Exception("Item not found");

        // query works as expected with postgres database, however when mocking it with sqlite in memory this extra check is needed.
        if (!withAnswers)
            item.Answers = new List<ItemAnswer>();

        if (!withReports)
            item.Reports = new List<Report>();

        return item;
    }

    /// <summary>
    /// Retrieve a range of items from the database
    /// </summary>
    /// <param name="offset">the id of an item from which we want to begin fetching</param>
    /// <param name="range">the number of items to fetch from the offset</param>
    /// <param name="maxQuestionBodyLength">the maximum length of the question body</param>
    /// <param name="sortColumn">the property to sort by</param>
    /// <param name="sortOrder">ascending or descending</param>
    /// <returns>a list of items and the node paths</returns>
    public async Task<Item[]> GetItems(
        int offset,
        int range,
        int maxQuestionBodyLength,
        string sortColumn = "id",
        string sortOrder = "ascending")
    {
        var query = context.Items.AsNoTracking();

        bool ascending = sortOrder == "ascending";

        IOrderedQueryable<Item> ordered = sortColumn.ToLower() switch
        {
            "active" => ApplySort(query, e => e.Active, ascending),
            "type" => ApplySort(query, e => e.Type, ascending),
            "lang" => ApplySort(query, e => e.Lang, ascending),
            "level" => ApplySort(query, e => e.Level, ascending),
            "source" => ApplySort(query, e => e.Source, ascending),
            "responsetype" => ApplySort(query, e => e.ResponseType, ascending),
            "appearancecount" => ApplySort(query, e => e.AppearanceCount, ascending),
            "questiontext" => ApplySort(query, e => e.QuestionText, ascending),
            _ => ApplySort(query, e => e.Id, ascending),
        };

        var items = await ordered
            .Skip(offset)
            .Take(range)
            .Select(item => new Item
            {
                Id = item.Id,
                Type = item.Type,
                Active = item.Active,
                // leave this logic here, abstracting substring logic away to the SmallItemDto class
                // will cause the EF to no longer optimize the resulting SQL query to use substring -> worse performance
                QuestionText =
                    item.QuestionText.Length > maxQuestionBodyLength ?
                        item.QuestionText.Substring(0, maxQuestionBodyLength) :
                        item.QuestionText,
                Level = item.Level,
                Lang = item.Lang,
                ResponseType = item.ResponseType,
                AppearanceCount = item.AppearanceCount,
                Source = item.Source,
                Scopes = item.Scopes
                    .Where(scope => scope.Type == ScopeType.Topic)
                    .ToList()
            })
            .ToArrayAsync();

        return items;
    }

    private static IOrderedQueryable<Item> ApplySort<TKey>(
    IQueryable<Item> query,
    Expression<Func<Item, TKey>> keySelector,
    bool ascending)
    {
        return ascending
            ? query.OrderBy(keySelector).ThenBy(e => e.Id)
            : query.OrderByDescending(keySelector).ThenByDescending(e => e.Id);
    }

    public async Task<Item[]> GetItemsForAssessment(int[] itemIds)
    {
        var items = await context.Items
            .AsNoTracking()
            .Where(item => itemIds.Contains(item.Id))
            .ToArrayAsync();

        if (items.Length != itemIds.Length)
        {
            var missing = itemIds.Except(items.Select(i => i.Id));
            throw new KeyNotFoundException($"Items with ids {string.Join(", ", missing)} were not found");
        }

        return items;
    }

    /// <summary>
    /// Query which returns the items seen by a user.
    /// </summary>
    /// <param name="userId">The given user</param>
    /// <param name="afterDate">Optional parameter. If a date is given. Only</param>
    /// <returns></returns>
    private IQueryable<Item> ItemsSeenByUser(int userId, DateTime? afterDate = null)
    {
        return (
            from userAnswer in context.UserAnswers
            join levelResult in context.LevelResults on userAnswer.LevelResultId equals levelResult.Id
            where levelResult.UserId == userId
                  && (!afterDate.HasValue || levelResult.CreatedAt >= afterDate)
            select userAnswer.Item
        ).Distinct();
    }

    /// <summary>
    /// Query which returns the items a user can answer. Making sure they do not see any repeat questions too early.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IQueryable<Item> NotRecentlySeenItems(int userId)
    {
        var date = DateTime.UtcNow.AddDays(-DaysTillRedo);
        return from item in context.Items
               // filter by already answered items within the timeframe
               where !ItemsSeenByUser(userId, date).Any(answeredItem => answeredItem.Id == item.Id)
               select item;
    }

    /// <summary>
    /// Get Item by its answer text.
    /// </summary>
    /// <param name="searchString"></param>
    /// <param name="MaxQuestionBodyLength"></param>
    /// <returns></returns>
    public async Task<SmallItemDto[]> GetItemByAnswertext(string searchString, int MaxQuestionBodyLength)
    {
        //Breaks Dto convention due to performance considerations, see comment below.
        var query =
            from item in context.Items
            where Regex.IsMatch(item.QuestionText, $"(?<!<img[^>])({searchString})(?![^<]>)", RegexOptions.IgnoreCase)
            select new SmallItemDto
            {
                Id = item.Id,
                Type = item.Type,
                Active = item.Active,
                // leave this logic here, abstracting substring logic away to the SmallItemDto class
                // will cause the EF to no longer optimize the resulting SQL query to use substring -> worse performance
                QuestionText =
                    item.QuestionText.Length > MaxQuestionBodyLength ?
                        item.QuestionText.Substring(0, MaxQuestionBodyLength) :
                        item.QuestionText,
                Level = item.Level,
                Lang = item.Lang,
                Topics = item.Scopes,
                ResponseType = item.ResponseType,
                AppearanceCount = item.AppearanceCount,
                Source = item.Source
            };

        return await query.ToArrayAsync();
    }


    /// <summary>
    /// Updates an item
    /// </summary>
    /// <param name="UpdatedItem">Item with updated parameters sent from frontend</param>
    /// <returns>The Updated Item</returns>
    /// <exception cref="Exception"></exception>
    public async Task<Item> UpdateItem(Item UpdatedItem)
    {
        var item =
            await context.Items.FirstOrDefaultAsync(i => i.Id == UpdatedItem.Id)
            ??
            throw new Exception("Item not found");
        if (UpdatedItem.Scopes != null)
        {
            int[] topicIds = [.. UpdatedItem.Scopes.Select(t => t.Id)];

            await topicService.UpdateTopicsForItem(item, topicIds);
        }

        item.Type = UpdatedItem.Type;
        item.Active = UpdatedItem.Active;
        item.Source = UpdatedItem.Source;
        item.ResponseType = UpdatedItem.ResponseType;
        item.Level = UpdatedItem.Level;
        item.AnswerExplanation = UpdatedItem.AnswerExplanation;
        item.QuestionText = UpdatedItem.QuestionText;
        item.Answers = UpdatedItem.Answers;
        item.Lang = UpdatedItem.Lang;

        await context.SaveChangesAsync();

        return item;
    }

    /// <summary>
    /// Activate an <see cref="Item"/>
    /// </summary>
    /// <param name="Id">The Id of the <see cref="Item"/> to activate</param>
    /// <returns>The activated <see cref="Item"/></returns>
    /// <exception cref="Exception">Something when wrong while activating the item</exception>
    public async Task<Item> ActivateItem(int Id)
    {
        var item =
            await context.Items.FirstOrDefaultAsync(i => i.Id == Id)
            ??
            throw new Exception("item not found");
        item.Active = true;
        await context.SaveChangesAsync();
        return item;
    }

    /// <summary>
    /// Deactivate an <see cref="Item"/>
    /// </summary>
    /// <param name="Id">The Id of the <see cref="Item"/> to deactivate</param>
    /// <returns>The deactivated <see cref="Item"/></returns>
    /// <exception cref="Exception"></exception>
    public async Task<Item> DeactivateItem(int Id)
    {
        var item =
            await context.Items.FirstOrDefaultAsync(i => i.Id == Id)
            ??
            throw new Exception("item not found");
        item.Active = false;
        await context.SaveChangesAsync();
        return item;
    }

    /// <summary>
    /// Returns the text of the correct answer of an item, if there is not any, returns an empty string
    /// </summary>
    /// <param name="itemId">The id of the item</param>
    public async Task<string> CorrectAnswer(int itemId)
    {
        try
        {
            ItemAnswer answer = await context.Answers.SingleAsync(answer => answer.ItemId == itemId && answer.Correct);
            return answer.AnswerText ?? "";
        }
        catch (InvalidOperationException e)
        {
            return "";
        }
    }

    /// <summary>
    /// Remove all reports of a certain item given it's Id
    /// </summary>
    /// <param name="Id">The Id of the item</param>
    /// <exception cref="Exception"></exception>
    public async Task ResolveReports(int Id)
    {
        var item =
            await context.Items.FirstOrDefaultAsync(i => i.Id == Id)
            ??
            throw new Exception("item not found"); 
        item.Reports = [];  

        await context.Reports
        .Where(r => r.ItemId == Id)
        .ExecuteDeleteAsync();

        await context.SaveChangesAsync();
    }   
}
