/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Models;
using API.Services;

namespace APITests.Services;

public class ResearcherDashboardServiceTests
{
    private static (AppDbContext Ctx, SqliteConnection Conn) CreateSqliteContext()
    {
        var conn = new SqliteConnection("Filename=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;

        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();

        return (ctx, conn);
    }

    [Fact(DisplayName = "getMetrics: Returns correct student count")]
    public async Task getMetrics_ReturnsCorrectStudentCount()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var userRole = new ApplicationRole { Name = "User" };
        context.Roles.Add(userRole);

        var user1 = new User { FirstName = "Alice", LastName = "Smith", DisplayName = "Alice", Email = "alice@test.com", UserName = "alice@test.com" };
        var user2 = new User { FirstName = "Bob", LastName = "Jones", DisplayName = "Bob", Email = "bob@test.com", UserName = "bob@test.com" };
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        context.UserRoles.AddRange(
            new Microsoft.AspNetCore.Identity.IdentityUserRole<int> { UserId = user1.Id, RoleId = userRole.Id },
            new Microsoft.AspNetCore.Identity.IdentityUserRole<int> { UserId = user2.Id, RoleId = userRole.Id }
        );
        await context.SaveChangesAsync();

        var service = new ResearcherDashboardService(context);

        var result = await service.getMetrics();

        Assert.Equal(2, result.StudentCount);
    }

    [Fact(DisplayName = "getMetrics: Returns correct active users past week count")]
    public async Task getMetrics_ReturnsCorrectActiveUsersPastWeek()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        context.Users.AddRange(
            new User { FirstName = "Alice", LastName = "Smith", DisplayName = "Alice", Email = "alice@test.com", UserName = "alice@test.com", LastLoggedIn = DateTimeOffset.UtcNow.AddDays(-1) },  // active
            new User { FirstName = "Bob", LastName = "Jones", DisplayName = "Bob", Email = "bob@test.com", UserName = "bob@test.com", LastLoggedIn = DateTimeOffset.UtcNow.AddDays(-10) },         // inactive
            new User { FirstName = "Charlie", LastName = "Brown", DisplayName = "Charlie", Email = "charlie@test.com", UserName = "charlie@test.com", LastLoggedIn = null }                        // never logged in
        );
        await context.SaveChangesAsync();

        var service = new ResearcherDashboardService(context);

        var result = await service.getMetrics();

        Assert.Equal(1, result.ActiveUserCount);
    }

    [Fact(DisplayName = "getMetrics: Returns correct tests taken past week")]
    public async Task getMetrics_ReturnsCorrectTestsTakenPastWeek()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var user = new User { FirstName = "Alice", LastName = "Smith", DisplayName = "Alice", Email = "alice@test.com", UserName = "alice@test.com" };
        var topic = new Scope { Name = "Test Topic" };
        context.Users.Add(user);
        context.Scopes.Add(topic);
        await context.SaveChangesAsync();

        context.LevelResults.AddRange(
            new LevelResult { UserId = user.Id, TopicId = topic.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },  // within past week
            new LevelResult { UserId = user.Id, TopicId = topic.Id, CreatedAt = DateTime.UtcNow.AddDays(-3) },  // within past week
            new LevelResult { UserId = user.Id, TopicId = topic.Id, CreatedAt = DateTime.UtcNow.AddDays(-10) }  // older than a week
        );
        await context.SaveChangesAsync();

        var service = new ResearcherDashboardService(context);

        var result = await service.getMetrics();

        var totalCount = result.TestsTakenPastWeek.Sum(d => d.Count);
        Assert.Equal(2, totalCount);
    }

    [Fact(DisplayName = "getMetrics: Returns 7 days in tests taken past week")]
    public async Task getMetrics_Returns7DaysInTestsTakenPastWeek()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var service = new ResearcherDashboardService(context);

        var result = await service.getMetrics();

        Assert.Equal(7, result.TestsTakenPastWeek.Count);
    }

    [Fact(DisplayName = "getMetrics: Returns correct flagged items count")]
    public async Task getMetrics_ReturnsCorrectFlaggedItemsCount()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var flaggedItem = new Item { QuestionText = "a", ResponseType = "a" };
        var unflaggedItem = new Item { QuestionText = "b", ResponseType = "b" };
        context.Items.AddRange(flaggedItem, unflaggedItem);
        context.Users.Add(CreateUser(1));
        await context.SaveChangesAsync();
        
        context.Reports.Add(new Report { ItemId = flaggedItem.Id, ItemError = ItemError.QTextEmpty, UserId = 1});
        await context.SaveChangesAsync();

        var service = new ResearcherDashboardService(context);

        var result = await service.getMetrics();

        Assert.Equal(1, result.FlaggedItemCount);
    }

    [Fact(DisplayName = "getMetrics: Returns zero counts on empty database")]
    public async Task getMetrics_ReturnsZeroOnEmptyDatabase()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var service = new ResearcherDashboardService(context);

        var result = await service.getMetrics();

        Assert.Equal(0, result.StudentCount);
        Assert.Equal(0, result.ActiveUserCount);
        Assert.Equal(0, result.TestsTakenPastWeek.Sum(d => d.Count));
        Assert.Equal(0, result.FlaggedItemCount);
    }
    
    private User CreateUser(int id = 1)
    {
        return new User
        {
            Id = id,
            DisplayName = $"user{id}",
            FirstName = "Test",
            LastName = "User",
            Email = $"user{id}@example.com",
            PasswordHash = "x"
        };
    }
}