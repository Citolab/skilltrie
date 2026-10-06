/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA;
using AA.UrningsAlgorithm;
using AA.PickItemsAlgorithm;
using AA.UserProficiencyAlgorithm;
using AA.UserMasteryAlgorithm;
using API.Handlers;
using API.Handlers.GameEventHandlers;
using Models;
using API.Tools.QTIConverting;
using API.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public interface ILevelService
{
    Task<(string, Item[], LevelResult)> GetRandomLevel(int userId, int topicId, Item.Language language);
    Task<LevelResult> GenerateRandom(User user, int topicId, Item.Language language);
    Task UpdateUserAnswer(UserAnswerRequest userAnswerRequest);
    Task<LevelResult> SubmitLevel(LevelAnswerRequest levelAnswerRequest, ILogger logger);
    Task<bool> UpdateUserMastery(int testId, ILogger logger);
    Task GrantLevelRewards(LevelResult level, int userId, int currencyId, int currencyGainPerCorrectAnswer = 5);
    Task<int?> GetLevelOwnerId(int levelId);
}

/// <summary>
/// Handles creation and updating of level results for users.
/// Provides methods to generate random level results, build results from items,
/// and update user answers.
/// </summary>
public class LevelService(
    AppDbContext context,
    IItemPoolService itemPoolService,
    ITopicService topicService,
    IItemService itemService,
    LevelHandler levelHandler,
    IUserCurrencyService userCurrencyService,
    IAlgorithmRegistry<IUserProficiencyAlgorithm> upaRegistry,
    IAlgorithmRegistry<IUserMasteryAlgorithm> umaRegistry,
    IAlgorithmRegistry<IUrningsAlgorithm> uaRegistry,
    IAlgorithmRegistry<IPickItemsAlgorithm> piRegistry,
    IGameEventManager gameEventManager,
    ILogger<LevelService> logger,
    TimeTool timeTool
    ) : ILevelService
{

    public async Task<(string, Item[], LevelResult)> GetRandomLevel(int userId, int topicId, Item.Language language)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found");

        await itemPoolService.QueueNewItemGeneration(topicId, userId); // Queue item generation if there might not be enough items available

        var level = await GenerateRandom(user, topicId, language);
        context.LevelResults.Add(level);
        await context.SaveChangesAsync();

        Item[] items = [.. level.UserAnswer.Select(a => a.Item)];

        string assessmentXml = await Task.Run(() => QTIXML.GenerateAssesment(items));

        return (assessmentXml, items, level);
    }

    /// <summary>
    /// Returns all the items that are available currently for the user,
    /// considering things as proficiency and recency and possibly more constraints in the future.
    /// </summary>
    /// <param name="user">The user to return the available items for</param>
    /// <param name="topicId">The ID of the topic from which to select the items</param>
    /// <param name="language">The language of the items to select</param>
    /// <returns></returns>
    private async Task<List<Item>> ItemsAvailableToUser(User user, int? topicId = null, Item.Language? language = Item.Language.Dutch)
    {
        var query = context.Items.AsQueryable();

        if (topicId.HasValue)
        {
            query = from item in query
                    join topicItem in context.ScopeItems on item.Id equals topicItem.ItemId
                    where topicItem.ScopeId == topicId.Value && item.Lang == language
                    select item;
        }

        return await query
            .OrderBy(_ => EF.Functions.Random())
            .ToListAsync();
    }

    /// <summary>
    /// Generates a random <see cref="LevelResult"/> for a user.
    /// Selects items randomly from the database, optionally restricted to specific topics.
    /// Throws an exception if there are not enough items available.
    /// </summary>
    /// <param name="user">The user for whom the level result is generated.</param>
    /// <param name="topicId">Topic Id for which the level will be created.</param>
    /// <param name="language">The language of the items to select.</param>
    /// <returns>A new <see cref="LevelResult"/> containing randomly selected items.</returns>
    public async Task<LevelResult> GenerateRandom(User user, int topicId, Item.Language language)
    {
        List<Item> items = await ItemsAvailableToUser(user, topicId, language);
        IPickItemsAlgorithm pickItemsAlgorithm = piRegistry.Get("V1PickItems");
        List<Item> itemsPicked = pickItemsAlgorithm.PickItems(items);

        return GenerateFromItems(itemsPicked, user, topicId);
    }

    /// <summary>
    /// Generate a LevelResult from the given parameters
    /// DOES NOT SAVE IN DB
    /// </summary>
    /// <param name="items">The Items to use in the LevelResult. Will be converted to UserAnswers</param>
    /// <param name="user"></param>
    /// <param name="topicId"></param>
    /// <returns></returns>
    private LevelResult GenerateFromItems(ICollection<Item> items, User user, int? topicId = null)
    {
        var userAnswers = items.Select(item => new UserAnswer { Item = item });
        var level = new LevelResult
        {
            User = user,
            UserAnswer = [.. userAnswers],
            TopicId = topicId is not null ? (int)topicId : 0
        };

        return level;
    }

    /// <summary>
    /// Updates or creates a user answer for a given item within a level result.
    /// Normalizes the answer text if necessary, updates existing answers, or adds new ones.
    /// </summary>
    /// <param name="userAnswerRequest">The <see cref="UserAnswerRequest"/> containing user answer details (item ID, level ID, answer, correctness, etc.).</param>
    /// <returns>An <see cref="IActionResult"/> indicating success or failure (e.g., NotFound if the item does not exist).</returns>
    public async Task UpdateUserAnswer(UserAnswerRequest userAnswerRequest)
    {
        var item = await context.Items
            .Include(i => i.Answers)
            .FirstOrDefaultAsync(i => i.Id == userAnswerRequest.ItemId);

        if (item == null)
            throw new KeyNotFoundException($"Item {userAnswerRequest.ItemId} not found");

        var existingAnswer = await context.UserAnswers.Include(userAnswer => userAnswer.LevelResult)
            .FirstOrDefaultAsync(a => a.ItemId == userAnswerRequest.ItemId && a.LevelResultId == userAnswerRequest.LevelId);

        if (existingAnswer == null) throw new KeyNotFoundException("answer not found");

        if (string.IsNullOrWhiteSpace(userAnswerRequest.Answer))
        {
            existingAnswer.Answer = item.Answers
                .Where(a => a.AnswerIdentifier == userAnswerRequest.AnswerIdentifier)
                .Select(a => a.AnswerText)
                .FirstOrDefault() ?? "";
        }
        else existingAnswer.Answer = userAnswerRequest.Answer;

        existingAnswer.AnswerIdentifier = userAnswerRequest.AnswerIdentifier;
        existingAnswer.Correct = userAnswerRequest.Correct;
        existingAnswer.CompletionStatus = userAnswerRequest.CompletionStatus;
        context.UserAnswers.Update(existingAnswer);

        await gameEventManager.TriggerGameEvent(
            new ItemAnsweredData { Data = existingAnswer, UserId = existingAnswer.LevelResult.UserId }
        );

        var userId = (
            from lr in context.LevelResults
            where lr.Id == userAnswerRequest.LevelId
            select lr.UserId
        ).Single();

        var topicIds = (
            from ua in context.UserAnswers
            join i in context.Items on ua.ItemId equals i.Id
            from t in i.Scopes
            select t.Id
        ).Distinct().ToList();

        var upaAlgorithm = upaRegistry.Get("Simple UPA");
        foreach (var topicId in topicIds)
            await upaAlgorithm.UpdateProficiency(userId, topicId);

        var uaAlgorithm = uaRegistry.Get("Basic Urnings Algorithm");
        try
        {
            await uaAlgorithm.UpdateUrnings(userId, item.Id, userAnswerRequest.Correct);
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to update urnings algorithm for user {userId}, " +
                                               $"because of the following error\n{e}");
        }
    }

    /// <summary>
    /// Submit a Level
    /// </summary>
    /// <param name="levelAnswerRequest">The <see cref="LevelAnswerRequest"/></param>
    /// <param name="logger"> The <see cref="ILogger"/> to log things</param>
    /// <returns>A <see cref="LevelResult"/></returns>
    /// <exception cref="Exception">Something went wrong while submitting the level</exception>
    public async Task<LevelResult> SubmitLevel(LevelAnswerRequest levelAnswerRequest, ILogger logger)
    {
        var level = await context.LevelResults
            .Include(l => l.UserAnswer)
            .FirstOrDefaultAsync(l => l.Id == levelAnswerRequest.LevelResultId);

        if (level == null)
            throw new Exception($"Level {levelAnswerRequest.LevelResultId} not found");

        foreach (UserAnswerRequest answer in levelAnswerRequest.Answers)
        {
            try
            {
                await UpdateUserAnswer(answer);
            }
            catch (Exception e)
            {
                logger.LogError($"Failed to register answer {answer.AnswerIdentifier} for user with id {level.UserId}, " +
                                $"due to the following error:\n {e}\n" +
                                $"Continuing with saving next answer");
                continue;
            }
        }
        await context.SaveChangesAsync();

        bool mastered = await UpdateUserMastery(levelAnswerRequest.LevelResultId, logger);

        await gameEventManager.TriggerGameEvent(new TestCompletedData
        { UserId = level.UserId, Level = level, Mastered = mastered });

        await UpdateUserStreak(level.UserId);

        await itemPoolService.QueueNewItemGeneration(levelAnswerRequest.LevelResultId);

        await context.SaveChangesAsync();

        return level;
    }

    /// <summary>
    /// Updates the user mastery in the database based on the test Id provided.
    /// </summary>
    /// <param name="testId">The level/test id</param>
    /// <param name="logger"> The <see cref="ILogger"/> to log things</param>
    public async Task<bool> UpdateUserMastery(int testId, ILogger logger)
    {
        var result = await (
            from lr in context.LevelResults
            where lr.Id == testId
            select new { lr.TopicId, lr.UserId }
        ).SingleAsync();

        int topicId = result.TopicId;
        int userId = result.UserId;

        var algorithm = umaRegistry.Get("Simple UMA");

        bool mastered = await algorithm.CalculateMastery(userId, topicId, testId);

        await topicService.UpdateUserTopicMastery(topicId, userId, mastered, logger);

        return mastered;
    }

    /// <summary>
    /// Called when a user completes a level, increments streak by one if it is the first level completed today.
    /// </summary>
    /// <param name="userId">The ID of the user whose streak is being updated.</param>
    /// <exception cref="Exception">Thrown if the user is not found or there is no streak data for the user.</exception>
    public async Task UpdateUserStreak(int userId)
    {
        var streak = await context.UserStreaks.FindAsync(userId)
            ?? throw new Exception("User not found, or there is no streak data for this user.");

        var today = timeTool.Today();

        if (today != streak.LastDayCompleted.Date)
        {
            streak.LastDayCompleted = today;
            streak.CurrentStreak += 1;

            if (streak.CurrentStreak > streak.HighestStreak)
                streak.HighestStreak = streak.CurrentStreak;

            await context.SaveChangesAsync();

            await gameEventManager.TriggerGameEvent(
                new StreakProlongedData { UserId = userId, Streak = streak });
        }
    }

    public async Task GrantLevelRewards(LevelResult level, int userId, int currencyId, int currencyGainPerCorrectAnswer)
    {
        var userCurrencies = await userCurrencyService.GetUserCurrencies(userId);
        if (userCurrencies.Any(uc => uc.CurrencyId == currencyId))
        {
            int correctCount = level.UserAnswer.Count(ua => ua.Correct);
            int currencyToAdd = correctCount * currencyGainPerCorrectAnswer;
            await userCurrencyService.UpdateUserCurrency(userId, currencyId, currencyToAdd);
        }
    }

    public async Task<int?> GetLevelOwnerId(int levelId)
    {
        return await context.LevelResults
            .Where(l => l.Id == levelId)
            .Select(l => (int?)l.UserId)
            .FirstOrDefaultAsync();
    }
}
