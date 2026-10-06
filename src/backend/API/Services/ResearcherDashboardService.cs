/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
using API.Controllers.DTOs;
using Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public interface IResearcherDashboardService
{
    Task<MetricDTO> getMetrics();
}

/// <summary>
/// Handles querying data from the database and processing it into a DTO
/// </summary>
public class ResearcherDashboardService(AppDbContext context) : IResearcherDashboardService
{
    public async Task<MetricDTO> getMetrics()
    {
        var userRoleId = await context.Roles
            .Where(r => r.Name == "User")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var studentCount = await context.UserRoles
            .CountAsync(ur => ur.RoleId == userRoleId);

        var oneWeekAgo = DateTime.UtcNow.AddDays(-6);

        var activeUserCount = (await context.Users
            .Select(u => u.LastLoggedIn)
            .ToListAsync())
            .Count(lastLoggedIn => lastLoggedIn != null && lastLoggedIn >= oneWeekAgo);

        var testsTakenPastWeek = await context.LevelResults
            .Where(t => t.CreatedAt >= oneWeekAgo)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new DailyCountDTO { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var allDays = Enumerable.Range(0, 7)
            .Select(i => oneWeekAgo.Date.AddDays(i))
            .Select(date => new DailyCountDTO
            {
                Date = date,
                Count = testsTakenPastWeek.FirstOrDefault(d => d.Date == date)?.Count ?? 0
            })
            .ToList();

        var flaggedItemCount = await context.Items
            .CountAsync(i => i.Reports.Any());

        return new MetricDTO
        {
            StudentCount = studentCount,
            ActiveUserCount = activeUserCount,
            TestsTakenPastWeek = allDays,
            FlaggedItemCount = flaggedItemCount
        };
    }
}