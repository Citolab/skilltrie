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


public class CurrencyServiceTests
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

    [Fact(DisplayName = "GetCurrency: returns currency when it exists")]
    public async Task GetCurrency_ReturnsCurrency_WhenExists()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        context.Currencies.Add(new Currency
        {
            Id = 20,
            Name = "Euro",
            Sprite = "test.png",
            StartingAmount = 100
            // set other required fields if your model enforces them
        });
        await context.SaveChangesAsync();

        var service = new CurrencyService(context);

        var cur = await service.GetCurrency(20);

        Assert.NotNull(cur);
        Assert.Equal(20, cur.Id);
        Assert.Equal("Euro", cur.Name);
        Assert.Equal("test.png", cur.Sprite);
        Assert.Equal(100, cur.StartingAmount);
    }

    [Fact(DisplayName = "GetCurrency: returns default nosecoin currency")]
    public async Task GetCurrency_ReturnsDefaultCurrency()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var service = new CurrencyService(context);

        var cur = await service.GetCurrency(1);

        Assert.NotNull(cur);
        Assert.Equal(1, cur.Id);
        Assert.Equal("NoseCoin", cur.Name);
    }

    [Fact(DisplayName = "GetCurrency: throws when currency does not exist")]
    public async Task GetCurrency_Throws_WhenNotFound()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var service = new CurrencyService(context);

        var ex = await Assert.ThrowsAsync<Exception>(() => service.GetCurrency(999));
        Assert.Equal("Currency not found.", ex.Message);
    }

    [Fact(DisplayName = "GetCurrencies: returns all currencies when any exist")]
    public async Task GetCurrencies_ReturnsAll_WhenAnyExist()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        context.Currencies.Add(new Currency
        {
            Id = 2,
            Name = "Euro",
            Sprite = "test.png",
            StartingAmount = 100
        });
        await context.SaveChangesAsync();

        var service = new CurrencyService(context);

        var curs = await service.GetCurrencies();

        Assert.NotNull(curs);
        Assert.Equal(2, curs.Count);
        Assert.Contains(curs, c => c.Id == 1);
        Assert.Contains(curs, c => c.Id == 2);
    }

    [Fact(DisplayName = "GetCurrencies: throws when no currencies exist")]
    public async Task GetCurrencies_Throws_WhenNoneExist()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var service = new CurrencyService(context);

        context.Currencies.RemoveRange(context.Currencies);
        await context.SaveChangesAsync();
        var ex = await Assert.ThrowsAsync<Exception>(() => service.GetCurrencies());

        Assert.Equal("No currencies defined", ex.Message);
    }
}



