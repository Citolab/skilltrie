/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers.DTOs;
using API.Tools.Badges.BadgeImageCollecting;
using Microsoft.EntityFrameworkCore;
using Models;

namespace API.Services;

public interface IBadgeAdminService
{
    Dictionary<string, List<BadgeParameterInfo>> GetParameterizedBadgeTypes();
    Task AddParameterizedBadge(AddParameterizedBadgeDTO dto);
    Task<List<Badge>> GetAllBadges();
    Task UpdateBadgeState(UpdateBadgeStateDTO dto);
    Task PublishBadge(string identifier);
    Task RemoveBadge(string identifier);
}

public class BadgeAdminService(
    AppDbContext context,
    IBadgeImageCollector collector,
    ILogger<IBadgeAdminService> logger
) : IBadgeAdminService
{
    /// <summary>
    /// Gets all the parameterized badge images and returns it in a dictionary with key its name and
    /// value its parameters. Only returns badge images which have more than 0 parameters
    /// </summary>
    public Dictionary<string, List<BadgeParameterInfo>> GetParameterizedBadgeTypes()
    {
        return collector
            .GetAllBadgeImageTypes()
            .ToDictionary(t => t.Name, t => t.GetParameters())
            .Where(e => e.Value.Count > 0)
            .ToDictionary();
    }

    /// <summary>
    /// Adds a parameterized badge entry constructed from the given dto to the database.
    /// Then notifies the badge image collector, so that it can update its collection.
    /// Does this in a transaction, so that if adding to the collection fails,
    /// the parameterized entry is not committed to the database.
    /// </summary>
    public async Task AddParameterizedBadge(AddParameterizedBadgeDTO dto)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            ParameterizedBadgeEntry newEntry = AddParameterizedBadgeDTO.ConstructEntryFromDto(dto);
            await context.ParameterizedBadgeEntries.AddAsync(newEntry);
            await context.SaveChangesAsync();

            newEntry.Topic = await context.Scopes.FirstOrDefaultAsync(t => t.Id == dto.TopicId);
            await collector.ParameterizedEntryAdded(newEntry);
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            logger.LogInformation("Adding parameterized entry endpoint failed\n" +
                                  "The following exception was thrown: {e}\n" +
                                  "Returning general BadHttpRequestException", e);
            await transaction.RollbackAsync();
            throw new BadHttpRequestException("Failed to add parameterized entry");
        }
    }

    /// <summary>
    /// A simple method to fetch all badges (except excluded ones), state included.
    /// </summary>
    /// <returns></returns>
    public async Task<List<Badge>> GetAllBadges()
    {
        return await context.Badges
            .Include(b => b.BadgeState)
            .Where(b => !b.BadgeState.Excluded)
            .ToListAsync();
    }

    /// <summary>
    /// Updates the <see cref="BadgeState"/> entry of the badge identified in the given dto.
    /// </summary>
    /// <param name="dto">The dto identifying the badge and the data which should be updated in the state</param>
    /// <exception cref="BadHttpRequestException">
    /// Is thrown in three scenarios:
    /// 1. No badge with this identifier has been found.
    /// 2. If the badge is found, but its phase is not <see cref="BadgePhase.Staging"/>
    /// 3. When OpenFrom is set on a later datetime than OpenLater</exception>
    public async Task UpdateBadgeState(UpdateBadgeStateDTO dto)
    {
        BadgeState? state = await context.BadgeStates.FindAsync(dto.Identifier);
        if (state == null) 
            throw new BadHttpRequestException("Could not find badge with given identifier", 404);
        if (state.Phase != BadgePhase.Staging)
            throw new BadHttpRequestException("Cannot update a badge which is not staging", 403);
        if (dto.OpenFrom >= dto.OpenUntil)
            throw new BadHttpRequestException("Bad request: OpenFrom is later than OpenLater", 400);
        
        state.OpenFrom = dto.OpenFrom;
        state.OpenUntil = dto.OpenUntil;
        state.FlagKey = dto.FlagKey;
        state.FlagVariant = dto.FlagVariant;
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Publishes a badge. Does this by setting <see cref="BadgeState.Phase"/> to <see cref="BadgePhase.Published"/>
    /// </summary>
    /// <param name="identifier">The identifier of the badge to publish</param>
    /// <exception cref="BadHttpRequestException">
    /// Is thrown in two scenarios:
    /// 1. No badge with this identifier has been found.
    /// 2. If the badge is found, but its phase is already <see cref="BadgePhase.Published"/>
    /// </exception>
    public async Task PublishBadge(string identifier)
    {
        BadgeState? state = await context.BadgeStates.FindAsync(identifier);
        if (state == null) 
            throw new BadHttpRequestException("Could not find badge with given identifier", 404);
        
        if (state.Phase == BadgePhase.Published)
            throw new BadHttpRequestException("Cannot publish a badge which is already published", 400);

        state.Phase = BadgePhase.Published;
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Calls the badge image collector to remove the badge with the given identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the badge to remove</param>
    /// <exception cref="BadHttpRequestException">
    /// Is thrown in two scenarios:
    /// 1. No badge with this identifier has been found.
    /// 2. If the badge is found, but its phase is not <see cref="BadgePhase.Staging"/>
    /// </exception>
    public async Task RemoveBadge(string identifier)
    {
        BadgeState? state = await context.BadgeStates.FindAsync(identifier);
        if (state == null) 
            throw new BadHttpRequestException("Could not find badge with given identifier", 404);
        
        if (state.Phase != BadgePhase.Staging)
            throw new BadHttpRequestException("Cannot remove a badge which is not staging", 403);

        await collector.RemoveBadge(identifier);
    }
}