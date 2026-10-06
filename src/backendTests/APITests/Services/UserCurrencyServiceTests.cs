/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Models;
using API.Services;
using API.Handlers;

namespace APITests.Services;

public class UserCurrencyServiceTests
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

        ctx.Users.AddRange(
            new User { Id = 1, FirstName = "John", LastName = "Pork", DisplayName = "JohnPork" },
            new User { Id = 2, FirstName = "Sara", LastName = "Pork", DisplayName = "SaraPork" }
        );
        ctx.SaveChanges();
        return (ctx, conn);
    }

    [Fact(DisplayName = "GetUserCurrencies: calls GetUser and returns only currencies for the requested user")]
    public async Task GetUserCurrencies_ForUser()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;


        context.UserCurrencies.AddRange(
            new UserCurrency { UserId = 1, CurrencyId = 1, Amount = 100 }, // adjust fields as needed
            new UserCurrency { UserId = 1, CurrencyId = 2, Amount = 50 },
            new UserCurrency { UserId = 2, CurrencyId = 1, Amount = 999 }
        );

        context.Currencies.Add(new Currency
        {
            Id = 2,
            Name = "Euro",
            Sprite = "test.png",
            StartingAmount = 100
        });
        await context.SaveChangesAsync();


        var userService = new Mock<IUserService>();
        var curService = new Mock<ICurrencyService>();
        UserCurrencyHandler ucurHandler = new();

        userService.Setup(s => s.GetUser(1))
                   .ReturnsAsync(new User { Id = 1 });

        var ucs = new UserCurrencyService(context, userService.Object, curService.Object, ucurHandler);

        var result = await ucs.GetUserCurrencies(1);

        userService.Verify(s => s.GetUser(1), Times.Once);

        Assert.Equal(2, result.Count);
        Assert.All(result, uc => Assert.Equal(1, uc.UserId));
        Assert.Contains(result, uc => uc.CurrencyId == 1);
        Assert.Contains(result, uc => uc.CurrencyId == 2);
    }

    [Fact(DisplayName = "GetUserCurrencies: returns empty list when user exists but has no currencies")]
    public async Task GetUserCurrencies_ReturnsEmpty_WhenNoneExist()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        // Seed other user's currencies only
        context.UserCurrencies.Add(new UserCurrency { UserId = 2, CurrencyId = 1, Amount = 999 });
        await context.SaveChangesAsync();

        var userService = new Mock<IUserService>();
        var curService = new Mock<ICurrencyService>();
        UserCurrencyHandler ucurHandler = new();

        userService.Setup(s => s.GetUser(1))
                   .ReturnsAsync(new User { Id = 1 });

        var ucs = new UserCurrencyService(context, userService.Object, curService.Object, ucurHandler);

        var result = await ucs.GetUserCurrencies(1);

        userService.Verify(s => s.GetUser(1), Times.Once);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "UpdateUserCurrency: updates existing user currency row")]
    public async Task UpdateUserCurrency_UpdatesExistingRow()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        context.UserCurrencies.Add(new UserCurrency { UserId = 1, CurrencyId = 1, Amount = 5 });
        await context.SaveChangesAsync();

        var userService = new Mock<IUserService>();
        userService.Setup(s => s.GetUser(1)).ReturnsAsync(new User { Id = 1 });

        var curService = new Mock<ICurrencyService>();

        var handler = new UserCurrencyHandler();

        var svc = new UserCurrencyService(context, userService.Object, curService.Object, handler);

        var updated = await svc.UpdateUserCurrency(1, 1, 20);

        userService.Verify(s => s.GetUser(1), Times.Once);
        curService.Verify(s => s.GetCurrency(1), Times.Once);

        Assert.Equal(1, await context.UserCurrencies.CountAsync());

        var dbRow = await context.UserCurrencies.SingleAsync(uc => uc.UserId == 1 && uc.CurrencyId == 1);
        Assert.Equal(25, dbRow.Amount);

        Assert.Equal(1, updated.UserId);
        Assert.Equal(1, updated.CurrencyId);
        Assert.Equal(25, updated.Amount);
    }

    [Fact(DisplayName = "UpdateUserCurrency: creates new row when none exists")]
    public async Task UpdateUserCurrency_CreatesNewRow_WhenNoneExists()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var userService = new Mock<IUserService>();
        var user = await context.Users.FindAsync(1);
        userService.Setup(s => s.GetUser(1)).ReturnsAsync(user!);

        var curService = new Mock<ICurrencyService>();
        var cur = await context.Currencies.FindAsync(1);
        curService.Setup(s => s.GetCurrency(1)).ReturnsAsync(cur!);

        var handler = new UserCurrencyHandler();

        var svc = new UserCurrencyService(context, userService.Object, curService.Object, handler);

        var created = await svc.UpdateUserCurrency(1, 1, 7);

        int newBalance = 7 + cur!.StartingAmount;

        userService.Verify(s => s.GetUser(1), Times.Once);
        curService.Verify(s => s.GetCurrency(1), Times.Once);

        Assert.Equal(1, await context.UserCurrencies.CountAsync());

        var dbRow = await context.UserCurrencies.SingleAsync(uc => uc.UserId == 1 && uc.CurrencyId == 1);
        Assert.Equal(newBalance, dbRow.Amount);

        Assert.Equal(1, created.UserId);
        Assert.Equal(1, created.CurrencyId);
        Assert.Equal(newBalance, created.Amount);
    }

    [Fact(DisplayName = "UpdateUserCurrency: when GetUser throws, the method throws and does not persist")]
    public async Task UpdateUserCurrency_Throws_WhenUserNotFound()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        // Optional: seed principals/rows - not required for this test

        var userService = new Mock<IUserService>();
        userService.Setup(s => s.GetUser(999)).ThrowsAsync(new Exception("User not found"));

        var curService = new Mock<ICurrencyService>();
        // Should never be called if GetUser throws first

        var handler = new UserCurrencyHandler();

        var svc = new UserCurrencyService(context, userService.Object, curService.Object, handler);

        var ex = await Assert.ThrowsAsync<Exception>(() => svc.UpdateUserCurrency(999, 1, 10));
        Assert.Equal("User not found", ex.Message);

        userService.Verify(s => s.GetUser(999), Times.Once);
        curService.Verify(s => s.GetCurrency(It.IsAny<int>()), Times.Never);

        Assert.Equal(0, await context.UserCurrencies.CountAsync());
    }

    [Fact(DisplayName = "UpdateUserCurrency: when GetCurrency throws, the method throws and does not persist")]
    public async Task UpdateUserCurrency_Throws_WhenCurrencyNotFound()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var userService = new Mock<IUserService>();
        userService.Setup(s => s.GetUser(1)).ReturnsAsync(new User { Id = 1 });

        var curService = new Mock<ICurrencyService>();
        curService.Setup(s => s.GetCurrency(999)).ThrowsAsync(new Exception("Currency not found"));

        var handler = new UserCurrencyHandler();

        var svc = new UserCurrencyService(context, userService.Object, curService.Object, handler);

        var ex = await Assert.ThrowsAsync<Exception>(() => svc.UpdateUserCurrency(1, 999, 10));
        Assert.Equal("Currency not found", ex.Message);

        userService.Verify(s => s.GetUser(1), Times.Once);
        curService.Verify(s => s.GetCurrency(999), Times.Once);

        Assert.Equal(0, await context.UserCurrencies.CountAsync());
    }
}
