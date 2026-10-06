/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Tools.Badges.BadgeImageCollecting;
using Microsoft.EntityFrameworkCore;
using Models;

namespace API.Tools.Badges.Compilation;

internal record PhaseSetting(BadgePhase phase, bool overwriteExisting);

/// <summary>
/// A class which can compile badges anytime at runtime. For now this is only done at application startup.
/// </summary>
public class LiveBadgeCompiler(AppDbContext context, ILogger<LiveBadgeCompiler> logger) : IBadgeImageCollectorListener
{
    /// <summary>
    /// Is called when the <see cref="BadgeImageCollector"/> has reset its badge image collection.
    /// Will first deactivate excluded badges (not in the collection).
    /// Then will handle each badge which is present in the collection.
    /// </summary>
    /// <param name="collectedBadgeImages">The list of <see cref="BadgeImage"/> which has been reset</param>
    public async Task AllBadgeImagesCollected(Dictionary<string, BadgeImage> collectedBadgeImages)
    {
        await DeactivateExcludedBadges(collectedBadgeImages);
        foreach (var collectedBadgeImage in collectedBadgeImages)
            await HandleBadgeImageAdded(
                collectedBadgeImage.Value,
                new PhaseSetting(BadgePhase.Published, overwriteExisting: false)
            );
    }

    /// <summary>
    /// Is called when a badge image has been added to the badge image collection.
    /// </summary>
    /// <param name="badgeImage">The badge image which was created from the parameterized entry</param>
    public async Task BadgeImageAdded(BadgeImage badgeImage)
    {
        await HandleBadgeImageAdded(
            badgeImage, 
            new PhaseSetting(BadgePhase.Staging, overwriteExisting: true)
        );
    }

    /// <summary>
    /// Fetches the badge from the database belonging to the badge image and removes it if it exists.
    /// Also removes the belonging parameterized entry if it exists. 
    /// </summary>
    /// <param name="badgeImage">The badge image which has been removed by the collector</param>
    public async Task BadgeImageRemoved(BadgeImage badgeImage)
    {
        string identifier = badgeImage.BadgeIdentifier();
        Badge? badge = await context.Badges.FindAsync(identifier);
        if (badge == null) return;
        
        context.Badges.Remove(badge);
        if (badgeImage.CreatedFrom != null)
            context.ParameterizedBadgeEntries.Remove(badgeImage.CreatedFrom);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Compiles the given badge image and checks whether it exists. If it does, it copies all the value of the compiled
    /// badge to the currently stored badge. It will also set the excluded property to false,
    /// might it have been excluded earlier. If it does exist, it will add the badge first, See <see cref="AddBadge"/>. <br/>
    /// Finally, it will also handle the trigger conditions of the badge, See <see cref="HandleBadgeTriggerConditions"/>
    /// and save the changes to the database.
    /// </summary>
    private async Task HandleBadgeImageAdded(BadgeImage badgeImage, PhaseSetting phaseSetting)
    {
        Badge badge = badgeImage.Compile();
        Badge? existing = await context.Badges.FindAsync(badge.Identifier);
        if (existing != null)
        {
            context.Entry(existing).CurrentValues.SetValues(badge);
            existing.TriggerConditions = badge.TriggerConditions;
            
            // expects a BadgeState mapping to be present, otherwise crashes
            BadgeState state = await context.BadgeStates.FindAsync(badge.Identifier) 
                               ?? throw new Exception(
                                   $"Could not find state of existing badge with identifier {badge.Identifier}"
                                );
            state.Excluded = false;
            if (phaseSetting.overwriteExisting) state.Phase = phaseSetting.phase;
        }
        else
            existing = AddBadge(badge, phaseSetting.phase);

        await HandleBadgeTriggerConditions(existing);
        
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes all the trigger conditions currently stored in the database which are associated to the given badge.
    /// Then adds the current trigger conditions to the database.
    /// </summary>
    /// <remarks>Does not actually save the badge to the database yet.
    /// A <see cref="AppDbContext.SaveChangesAsync(CancellationToken)"/>
    /// needs to be called by the method using this method</remarks>
    private async Task HandleBadgeTriggerConditions(Badge badge)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // first remove all previous trigger conditions, if there were any
            await context.Database.ExecuteSqlRawAsync("""
                DELETE FROM "BadgeTriggerCondition"
                       WHERE "BadgeIdentifier" = {0} 
            """, badge.Identifier);
            
            // tell ef core to forget their existences
            context.ChangeTracker
                .Entries<BadgeTriggerCondition>()
                .Where(e => e.Entity.BadgeIdentifier == badge.Identifier)
                .ToList().ForEach(entry => entry.State = EntityState.Detached);
            
            // then add the collected trigger conditions
            await context.BadgeTriggerConditions.AddRangeAsync(badge.TriggerConditions.Select(t =>
                new BadgeTriggerCondition
                {
                    BadgeIdentifier = badge.Identifier, TriggerCondition = t
                }));

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to insert triggers for badge {badge.Identifier}\nerror: {e}");
        }
    }

    /// <summary>
    /// Adds the given badge to the context. Also adds the one-to-one <see cref="BadgeState"/> mapping.
    /// </summary>
    /// <param name="badge">The badge to add</param>
    /// <param name="phase">The phase to initialize the badge with</param>
    /// <returns>The badge which is tracked by the context</returns>
    /// <remarks>Does not actually save the badge to the database yet.
    /// A <see cref="AppDbContext.SaveChangesAsync(CancellationToken)"/>
    /// needs to be called by the method using this method</remarks>
    private Badge AddBadge(Badge badge, BadgePhase phase = BadgePhase.Staging)
    {
        Badge trackedBadge = context.Badges.Add(badge).Entity;
        context.BadgeStates.Add(new BadgeState { BadgeIdentifier = badge.Identifier, Phase = phase});
        return trackedBadge;
    }

    /// <summary>
    /// Deactivates all the badges which are currently in the database, but are not in the collectedBadgeImages anymore.
    /// Will do so, by setting the Excluded property to true
    /// </summary>
    /// <remarks>Does not actually save the badge to the database yet.
    /// A <see cref="AppDbContext.SaveChangesAsync(CancellationToken)"/>
    /// needs to be called by the method using this method</remarks>
    private async Task DeactivateExcludedBadges(Dictionary<string, BadgeImage> collectedBadgeImages)
    {
        List<Badge> currentBadges = await context.Badges
            .Include(b => b.BadgeState).ToListAsync();
        IEnumerable<Badge> excludedBadges = currentBadges
            .Where(b => !collectedBadgeImages.ContainsKey(b.Identifier));
        
        foreach (var excludedBadge in excludedBadges)
            excludedBadge.BadgeState.Excluded = true; // expects a BadgeState mapping to be present, otherwise crashes
    }
}