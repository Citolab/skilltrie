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

public class CosmeticServiceTests
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

        ctx.Cosmetics.AddRange(new Cosmetic
        {
            Id = 1,
            Name = "Top Hat",
            ClothingType = ClothingType.Hat,
            DefaultOwned = false,
            Price = 25,
            CurrencyId = 1,
            RiveFile = "test.riv",
            RiveArtboard = "Board",
            RiveStateMachine = "State",
            RiveInput = "RiveInput",
            IconFile = "icon.jpg"
        },
        new Cosmetic
        {
            Id = 2,
            Name = "Shirt",
            ClothingType = ClothingType.Shirt,
            DefaultOwned = true,
            Price = 25,
            CurrencyId = 1,
            RiveFile = "test.riv",
            RiveArtboard = "Board",
            RiveStateMachine = "State",
            RiveInput = "RiveInput",
            IconFile = "icon.jpg"
        });

        ctx.SaveChanges();
        return (ctx, conn);
    }

    [Fact(DisplayName = "GetCosmetic: returns cosmetic when it exists")]
    public async Task GetCosmetic_ReturnsCosmetic_WhenExists()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var service = new CosmeticService(context);

        var cos = await service.GetCosmetic(1);

        Assert.NotNull(cos);
        Assert.Equal(1, cos.Id);
        Assert.Equal("Top Hat", cos.Name);
        Assert.Equal(ClothingType.Hat, cos.ClothingType);
        Assert.False(cos.DefaultOwned);
        Assert.Equal(25, cos.Price);
        Assert.Equal(1, cos.CurrencyId);
    }

    [Fact(DisplayName = "GetCosmetic: throws when cosmetic does not exist")]
    public async Task GetCosmetic_Throws_WhenNotFound()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var service = new CosmeticService(context);

        var ex = await Assert.ThrowsAsync<Exception>(() => service.GetCosmetic(999));
        Assert.Equal("Cosmetic not found", ex.Message);
    }

    [Fact(DisplayName = "GetCosmetics: returns all cosmetics when type is null")]
    public async Task GetCosmetics_ReturnsAll_WhenTypeNull()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;
        var service = new CosmeticService(context);

        var result = await service.GetCosmetics();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Id == 1);
        Assert.Contains(result, c => c.Id == 2);
    }

    [Fact(DisplayName = "GetCosmetics: returns only cosmetics for requested ClothingType")]
    public async Task GetCosmetics_FiltersByType_WhenProvided()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;
        var service = new CosmeticService(context);

        var result = await service.GetCosmetics(ClothingType.Hat);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
        Assert.All(result, c => Assert.Equal(ClothingType.Hat, c.ClothingType));
    }

    [Fact(DisplayName = "GetCosmetics: returns empty list when none exist")]
    public async Task GetCosmetics_ReturnsEmpty_WhenNoneExist()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        context.Cosmetics.RemoveRange(context.Cosmetics);
        await context.SaveChangesAsync();

        var service = new CosmeticService(context);

        var result = await service.GetCosmetics();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetDefaultCosmetics: returns only DefaultOwned cosmetics")]
    public async Task GetDefaultCosmetics_ReturnsOnlyDefaults()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;
        var service = new CosmeticService(context);

        var defaults = await service.GetDefaultCosmetics();

        Assert.NotNull(defaults);
        Assert.Single(defaults);
        Assert.All(defaults, c => Assert.True(c.DefaultOwned));
        Assert.Equal(2, defaults.First().Id);
    }
}
