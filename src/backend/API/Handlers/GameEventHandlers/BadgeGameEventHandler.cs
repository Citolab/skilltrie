/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA;
using AA.StampAlgorithm;
using API.Handlers.BadgeHandlers;
using API.Services;
using API.Tools.Badges;
using API.Tools.Badges.BadgeImageCollecting;
using API.Tools;
using API.Tools.Badges.ABTesting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using Models;

namespace API.Handlers.GameEventHandlers;

/// <summary>
/// Wakes up when a game event has been triggered.
/// Will handle all the needed processing regarding badges corresponding the game event.
/// </summary>
public class BadgeGameEventHandler(AppDbContext context, 
    IBadgeImageCollector collector, 
    IAlgorithmRegistry<IStampAlgorithm> _registry,
    TimeTool timeTool,
    ILogger<BadgeGameEventHandler> logger,
    IAbTestingService abTestingService) : IGameEventListener
{
    /// <summary>
    /// Analyses the triggered game event and based on that gets a list of <see cref="BadgeTriggerCondition"/>,
    /// which indicate which badge image should be triggered.
    /// Tries to get the corresponding badge image from the <see cref="BadgeImageCollector"/> and
    /// handles the badge progression.
    /// </summary>
    /// <remarks>Skips game events where no user id is present as badges correspond to users</remarks>
    public async Task GameEventTriggered(GameEventData gameEventData)
    {
        if (gameEventData.UserId == null) return;
        GameEvent gameEvent = gameEventData.GameEventTriggered();
        Dictionary<string, BadgeImage> badgeImageCollection = collector.GetBadgeImageCollection();

        List<BadgeTriggerCondition> activatedBadges = await CollectActivatedBadges(gameEvent);
        activatedBadges = await PerformABFilter(activatedBadges, (int)gameEventData.UserId);
        
        foreach (BadgeTriggerCondition activatedBadge in activatedBadges)
        {
            BadgeImage? badgeImage = badgeImageCollection.GetValueOrDefault(activatedBadge.BadgeIdentifier);
            if (badgeImage is null) BadgeImageNotFoundFor(activatedBadge, gameEvent);
            else
                try { await HandleBadgeProgress(badgeImage, gameEventData); } 
                catch (Exception e) { BadgeProgressUpdateFailed(gameEventData, activatedBadge, e); }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Filters out triggered badgeTriggerConditions which don't pass the AB-test for this specific user.
    /// </summary>
    /// <param name="activatedBadges">The badges triggered</param>
    /// <param name="userId">The user id for which the game event is triggered, needed for ab-testing</param>
    private async Task<List<BadgeTriggerCondition>> PerformABFilter(List<BadgeTriggerCondition> activatedBadges, int userId)
    {
        return await activatedBadges.FilterABTesting(
            t => t.Badge.BadgeState, 
            userId, 
            abTestingService
        );
    }

    /// <summary>
    /// Looks in the database for all the <see cref="BadgeTriggerCondition"/> entries and filters the ones
    /// which have their TriggerCondition column set to the game event. <br/>
    /// Also filters out badges which are not open.
    /// </summary>
    private async Task<List<BadgeTriggerCondition>> CollectActivatedBadges(GameEvent gameEvent)
    {
        return await context.BadgeTriggerConditions
            .Include(b => b.Badge.BadgeState)
            .Where(b => b.Badge.BadgeState.Phase == BadgePhase.Published)
            .Where(b => !b.Badge.BadgeState.Excluded)
            .Where(b => 
                (b.Badge.BadgeState.OpenFrom == null || timeTool.Now() > b.Badge.BadgeState.OpenFrom) &&
                (b.Badge.BadgeState.OpenUntil == null || timeTool.Now() < b.Badge.BadgeState.OpenUntil)
            )
            .Where(b => b.TriggerCondition == gameEvent)
            .ToListAsync();
    }

    /// <summary>
    /// Fetches the unique badge progress of the badge with identifier "badgeIdentifier" and userId "userId".
    /// If not available, it adds one to the context and returns the tracked entity.
    /// </summary>
    private async Task<BadgeProgress> GetBadgeProgress(int userId, string badgeIdentifier)
    {
        BadgeProgress? badgeProgress = await context.BadgeProgresses
            .Include(b => b.Badge)
            .Include(b => b.Stamp)
            .FirstOrDefaultAsync(
                b => b.UserId == userId && b.BadgeIdentifier == badgeIdentifier
            );

        if (badgeProgress == null) 
            return (await context.BadgeProgresses.AddAsync(new BadgeProgress
            {
                UserId = userId, BadgeIdentifier = badgeIdentifier
            })).Entity;

        return badgeProgress;
    }

    /// <summary>
    /// Gets the badge progress corresponding to the badge image and sets the new progress of it
    /// by calling <see cref="IBadgeHandler.CalculateNewProgress"/>.
    /// Also determines whether the badge has been completed.
    /// </summary>
    /// <remarks>Ignores badges which have already been completed</remarks>
    private async Task HandleBadgeProgress(IBadgeHandler badgeImage, GameEventData data)
    {
        BadgeProgress progress = await GetBadgeProgress((int)data.UserId!, badgeImage.BadgeIdentifier());
        Badge badge = progress.Badge;
        if (progress.Stamp != null) return; // already accomplished
        
        int newProgress = badgeImage.CalculateNewProgress(data, progress);
        progress.Progress = newProgress;
        
        bool completed = newProgress >= badge.ProgressNeeded;
        if (completed)
        {
            IStampAlgorithm stampAlgorithm = _registry.Get("Simple stamp");
            BadgeStamp stamp = await stampAlgorithm.FindNewStampPosition((int)data.UserId!);
            stamp.DateAccomplished = timeTool.Now();
            progress.Stamp = stamp;
        }

        await context.SaveChangesAsync();
    }

    private void BadgeImageNotFoundFor(BadgeTriggerCondition activatedBadge, GameEvent activatedBy)
    {
        logger.LogError(
            $"Badge {activatedBadge.BadgeIdentifier} was triggered by GameEvent " +
            $"{activatedBy.GetDisplayName()}, but there was no badge image found " +
            $"to handle it");
    }

    private void BadgeProgressUpdateFailed(GameEventData gameEventData, BadgeTriggerCondition activatedBadge, Exception e)
    {
        logger.LogError($"Failed to update the badge progress for badge {activatedBadge.BadgeIdentifier}, " +
                         $"user {gameEventData.UserId}. Did the badge get removed in the mean time?\n" +
                         $"Exception thrown:\n" +
                         $"{e}");
    }
}