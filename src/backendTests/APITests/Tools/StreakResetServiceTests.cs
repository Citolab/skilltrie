/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Tools;
using API.Tools.Streak;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Models;

namespace APITests.Tools;

public class StreakResetServiceTests
{
    private readonly AppDbContext db;
    private readonly StreakResetService service;

    // Fix "today" so streak logic is deterministic
    private static readonly DateTime FakeNow = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc);

    public StreakResetServiceTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        db = new AppDbContext(options);
        db.Database.EnsureCreated();

        var timeTool = new TimeTool(timeProvider: new FakeTimeProvider(FakeNow));
        service = new StreakResetService(db, timeTool);
    }

    [Fact]
    public async Task ResetStreaks_ResetsLapsedStreaks()
    {
        db.Users.AddRange(
            new User { Id = 1, FirstName = "John", LastName = "Pork", Email = "john@test.com", UserName = "john@test.com", DisplayName = "JohnPork" },
            new User { Id = 2, FirstName = "Sara", LastName = "Pork", Email = "sara@test.com", UserName = "sara@test.com", DisplayName = "SaraPork" },
            new User { Id = 3, FirstName = "Mike", LastName = "Pork", Email = "mike@test.com", UserName = "mike@test.com", DisplayName = "MikePork" }
        );
        await db.SaveChangesAsync();

        db.UserStreaks.AddRange(
            new UserStreak { UserId = 1, CurrentStreak = 5, LastDayCompleted = FakeNow.AddDays(-2) }, // lapsed
            new UserStreak { UserId = 2, CurrentStreak = 3, LastDayCompleted = FakeNow.AddDays(-1) }, // still active
            new UserStreak { UserId = 3, CurrentStreak = 0, LastDayCompleted = FakeNow.AddDays(-3) }  // already 0
        );
        await db.SaveChangesAsync();

        await service.ResetStreaks();

        var streaks = await db.UserStreaks.ToListAsync();
        Assert.Equal(0, streaks.First(s => s.UserId == 1).CurrentStreak); // lapsed, reset
        Assert.Equal(3, streaks.First(s => s.UserId == 2).CurrentStreak); // active, untouched
        Assert.Equal(0, streaks.First(s => s.UserId == 3).CurrentStreak); // already 0, untouched
    }

    [Fact]
    public async Task ResetStreaks_NoStreaks_DoesNotThrow()
    {
        await service.ResetStreaks(); // empty DB, should be a no-op
    }
}
