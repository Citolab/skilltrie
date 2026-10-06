/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers;
using API.Handlers.GameEventHandlers;
using API.Tools.EventQueue;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Models;
using Npgsql;

namespace API.Services;

public interface IReportService
{
    Task CreateReport(int itemId, ItemError itemError, int userId);
    Task<List<Report>> GetAllReports();
}
public class ReportService(
    AppDbContext context,
    IItemService itemService,
    ReportHandler reportHandler,
    IEventQueue eventQueue,
    IGameEventManager gameEventManager,
    ILogger<IReportService> logger) : IReportService
{
    /// <summary>
    /// Create a new Report entry
    /// </summary>
    /// <param name="itemId">The Id of the <see cref="Item"/> that the report is about</param>
    /// <param name="itemError">The error concerning to this particular report</param>
    /// <param name="userId"></param>
    public async Task CreateReport(int itemId, ItemError itemError, int userId)
    {
        Item item = await itemService.VerifyItemExists(itemId);
        
        Report newReport = reportHandler.CreateReportForDb(itemId, itemError, userId);

        context.Reports.Add(newReport);
        try
        {
            await context.SaveChangesAsync();
        }
        // unique constraint failed exception
        catch (DbUpdateException e) when (
            e.InnerException is PostgresException { SqlState: "23505" } or SqliteException {SqliteErrorCode: 19}
        )
        {
            logger.LogTrace($"User with id {userId} has made a duplicate report on item with id {itemId}");
            return;
        }
        
        await eventQueue.QueueAsync(new EventData(
            EventType.ItemReported, 
            new Dictionary<string, int> {{"itemId", itemId}}
        ));

        await gameEventManager.TriggerGameEvent(new ItemReportedData { UserId = userId, Item = item });
    }

    public async Task<List<Report>> GetAllReports()
    {
        return await context.Reports.ToListAsync();
    }
}
