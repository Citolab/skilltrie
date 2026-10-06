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

public class UserCosmeticServiceTests
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
        }, new Cosmetic
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

    [Fact(DisplayName = "GetUserCosmetic: returns user's cosmetic when it exists")]
    public async Task GetUserCosmetic_Returns_WhenOwned()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        context.UserCosmetics.Add(new UserCosmetic { UserId = 1, CosmeticId = 1, Equipped = false });
        await context.SaveChangesAsync();

        var userService = new Mock<IUserService>();
        var cosmeticService = new Mock<ICosmeticService>();
        var userCurrencyService = new Mock<IUserCurrencyService>();
        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        var result = await svc.GetUserCosmetic(1, 1);

        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal(1, result.CosmeticId);
        Assert.NotNull(result.Cosmetic);
        Assert.Equal("Top Hat", result.Cosmetic.Name);
    }

    [Fact(DisplayName = "GetUserCosmetic: throws when user does not have cosmetic")]
    public async Task GetUserCosmetic_Throws_WhenNotOwned()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var userService = new Mock<IUserService>();
        var cosmeticService = new Mock<ICosmeticService>();
        var userCurrencyService = new Mock<IUserCurrencyService>();
        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        var ex = await Assert.ThrowsAsync<Exception>(() => svc.GetUserCosmetic(1, 2));
        Assert.Equal("User does not have this cosmetic", ex.Message);
    }

    [Fact(DisplayName = "GetUserCosmetics: calls VerifyUserExists and returns only cosmetics for the requested user")]
    public async Task GetUserCosmetics_ForUser_CallsVerifyUserExists()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        context.UserCosmetics.AddRange(
            new UserCosmetic { UserId = 1, CosmeticId = 1, Equipped = true },
            new UserCosmetic { UserId = 1, CosmeticId = 2, Equipped = false },
            new UserCosmetic { UserId = 2, CosmeticId = 2, Equipped = true }
        );
        await context.SaveChangesAsync();

        var userService = new Mock<IUserService>();
        userService.Setup(s => s.VerifyUserExists(1)).Returns(Task.CompletedTask);

        var cosmeticService = new Mock<ICosmeticService>();
        var userCurrencyService = new Mock<IUserCurrencyService>();
        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        var result = await svc.GetUserCosmetics(1);

        userService.Verify(s => s.VerifyUserExists(1), Times.Once);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, uc => Assert.Equal(1, uc.UserId));
        Assert.Contains(result, uc => uc.CosmeticId == 1);
        Assert.Contains(result, uc => uc.CosmeticId == 2);
    }

    [Fact(DisplayName = "GetUserCosmetics: filters by equipped when provided")]
    public async Task GetUserCosmetics_FiltersByEquipped()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        context.UserCosmetics.AddRange(
            new UserCosmetic { UserId = 1, CosmeticId = 1, Equipped = false },
            new UserCosmetic { UserId = 1, CosmeticId = 2, Equipped = true }
        );
        await context.SaveChangesAsync();

        var userService = new Mock<IUserService>();
        userService.Setup(s => s.VerifyUserExists(1)).Returns(Task.CompletedTask);

        var cosmeticService = new Mock<ICosmeticService>();
        var userCurrencyService = new Mock<IUserCurrencyService>();
        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        var equippedOnly = await svc.GetUserCosmetics(1, equipped: true);

        Assert.Single(equippedOnly);
        Assert.All(equippedOnly, uc => Assert.True(uc.Equipped));
        Assert.Equal(2, equippedOnly.First().CosmeticId);
    }

    [Fact(DisplayName = "GetUserCosmetics: filters by ClothingType when provided")]
    public async Task GetUserCosmetics_FiltersByType()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        context.UserCosmetics.AddRange(
            new UserCosmetic { UserId = 1, CosmeticId = 1, Equipped = true },
            new UserCosmetic { UserId = 1, CosmeticId = 2, Equipped = false }
        );
        await context.SaveChangesAsync();

        var userService = new Mock<IUserService>();
        userService.Setup(s => s.VerifyUserExists(1)).Returns(Task.CompletedTask);

        var cosmeticService = new Mock<ICosmeticService>();
        var userCurrencyService = new Mock<IUserCurrencyService>();
        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        var hats = await svc.GetUserCosmetics(1, type: ClothingType.Hat);

        Assert.Single(hats);
        Assert.Equal(1, hats.First().CosmeticId);
    }

    [Fact(DisplayName = "GetUserCosmetics: returns empty list when user exists but has none")]
    public async Task GetUserCosmetics_ReturnsEmpty_WhenNone()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var userService = new Mock<IUserService>();
        userService.Setup(s => s.VerifyUserExists(1)).Returns(Task.CompletedTask);

        var cosmeticService = new Mock<ICosmeticService>();
        var userCurrencyService = new Mock<IUserCurrencyService>();
        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        var result = await svc.GetUserCosmetics(1);

        userService.Verify(s => s.VerifyUserExists(1), Times.Once);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "BuyUserCosmetic: creates user cosmetic and charges currency when not owned")]
    public async Task BuyUserCosmetic_Creates_AndCharges_WhenNotOwned()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var user = await context.Users.FindAsync(1);
        var cos = await context.Cosmetics.FindAsync(2); // Fancy Hat (Price 10, CurrencyId 1)

        var userService = new Mock<IUserService>();
        userService.Setup(s => s.GetUser(1)).ReturnsAsync(user!);

        var cosmeticService = new Mock<ICosmeticService>();
        cosmeticService.Setup(s => s.GetCosmetic(2)).ReturnsAsync(cos!);

        var userCurrencyService = new Mock<IUserCurrencyService>();
        userCurrencyService.Setup(s => s.UpdateUserCurrency(1, cos!.CurrencyId, -cos.Price))
                           .ReturnsAsync(new UserCurrency { UserId = 1, CurrencyId = cos!.CurrencyId, Amount = 0 });

        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        var created = await svc.BuyUserCosmetic(1, 2);

        userService.Verify(s => s.GetUser(1), Times.Once);
        cosmeticService.Verify(s => s.GetCosmetic(2), Times.Once);
        userCurrencyService.Verify(s => s.UpdateUserCurrency(1, cos!.CurrencyId, -cos.Price), Times.Once);

        Assert.Equal(1, await context.UserCosmetics.CountAsync());
        var dbRow = await context.UserCosmetics.SingleAsync(uc => uc.UserId == 1 && uc.CosmeticId == 2);

        Assert.Equal(dbRow.UserId, created.UserId);
        Assert.Equal(dbRow.CosmeticId, created.CosmeticId);
    }

    [Fact(DisplayName = "BuyUserCosmetic: throws when user already owns cosmetic and does not charge")]
    public async Task BuyUserCosmetic_Throws_WhenAlreadyOwned()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        context.UserCosmetics.Add(new UserCosmetic { UserId = 1, CosmeticId = 2, Equipped = false });
        await context.SaveChangesAsync();

        var user = await context.Users.FindAsync(1);
        var cos = await context.Cosmetics.FindAsync(2);

        var userService = new Mock<IUserService>();
        userService.Setup(s => s.GetUser(1)).ReturnsAsync(user!);

        var cosmeticService = new Mock<ICosmeticService>();
        cosmeticService.Setup(s => s.GetCosmetic(2)).ReturnsAsync(cos!);

        var userCurrencyService = new Mock<IUserCurrencyService>();
        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        var ex = await Assert.ThrowsAsync<Exception>(() => svc.BuyUserCosmetic(1, 2));
        Assert.Equal("User already owns cosmetic", ex.Message);

        userCurrencyService.Verify(
            s => s.UpdateUserCurrency(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);

        Assert.Equal(1, await context.UserCosmetics.CountAsync());
    }

    [Fact(DisplayName = "EquipUserCosmetic: equips cosmetic (and relies on handler logic for same-type items)")]
    public async Task EquipUserCosmetic_UpdatesEquippedState()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        context.UserCosmetics.AddRange(
            new UserCosmetic { UserId = 1, CosmeticId = 1, Equipped = false }
        );
        await context.SaveChangesAsync();

        var userService = new Mock<IUserService>();
        var cosmeticService = new Mock<ICosmeticService>();
        var userCurrencyService = new Mock<IUserCurrencyService>();
        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        await svc.EquipUserCosmetic(1, 1, equipped: true);

        var target = await context.UserCosmetics
            .Include(uc => uc.Cosmetic)
            .SingleAsync(uc => uc.UserId == 1 && uc.CosmeticId == 1);

        Assert.True(target.Equipped);
    }

    [Fact(DisplayName = "EquipUserCosmetic: throws when user does not have cosmetic")]
    public async Task EquipUserCosmetic_Throws_WhenNotOwned()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var userService = new Mock<IUserService>();
        var cosmeticService = new Mock<ICosmeticService>();
        var userCurrencyService = new Mock<IUserCurrencyService>();
        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        var ex = await Assert.ThrowsAsync<Exception>(() => svc.EquipUserCosmetic(1, 3, equipped: true));
        Assert.Equal("User does not have this cosmetic", ex.Message);
    }

    [Fact(DisplayName = "DeleteUserCosmetic: removes row when owned")]
    public async Task DeleteUserCosmetic_RemovesRow_WhenOwned()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        context.UserCosmetics.Add(new UserCosmetic { UserId = 1, CosmeticId = 2, Equipped = true });
        await context.SaveChangesAsync();

        var userService = new Mock<IUserService>();
        var cosmeticService = new Mock<ICosmeticService>();
        var userCurrencyService = new Mock<IUserCurrencyService>();
        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        await svc.DeleteUserCosmetic(1, 2);

        Assert.Equal(0, await context.UserCosmetics.CountAsync());
    }

    [Fact(DisplayName = "DeleteUserCosmetic: throws when not owned")]
    public async Task DeleteUserCosmetic_Throws_WhenNotOwned()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        var userService = new Mock<IUserService>();
        var cosmeticService = new Mock<ICosmeticService>();
        var userCurrencyService = new Mock<IUserCurrencyService>();
        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        var ex = await Assert.ThrowsAsync<Exception>(() => svc.DeleteUserCosmetic(1, 2));
        Assert.Equal("User does not have this cosmetic", ex.Message);

        Assert.Equal(0, await context.UserCosmetics.CountAsync());
    }
    [Fact(DisplayName = "SellUserCosmetic: credits currency then deletes cosmetic (happy path)")]
    public async Task SellUserCosmetic_CreditsAndDeletes()
    {
        var (context, connection) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = connection;

        // Owned cosmetic to sell
        context.UserCosmetics.Add(new UserCosmetic { UserId = 1, CosmeticId = 2, Equipped = false });
        await context.SaveChangesAsync();

        var cos = await context.Cosmetics.FindAsync(2); // Fancy Hat (Price 10, CurrencyId 1)

        var userService = new Mock<IUserService>();
        var cosmeticService = new Mock<ICosmeticService>();
        cosmeticService.Setup(s => s.GetCosmetic(2)).ReturnsAsync(cos!);

        var userCurrencyService = new Mock<IUserCurrencyService>();
        userCurrencyService.Setup(s => s.UpdateUserCurrency(1, cos!.CurrencyId, cos.Price))
                           .ReturnsAsync(new UserCurrency { UserId = 1, CurrencyId = cos!.CurrencyId, Amount = 0 });

        var handler = new UserCosmeticHandler();

        var svc = new UserCosmeticService(
            context,
            userService.Object,
            cosmeticService.Object,
            userCurrencyService.Object,
            handler);

        await svc.SellUserCosmetic(1, 2);

        cosmeticService.Verify(s => s.GetCosmetic(2), Times.Once);
        userCurrencyService.Verify(s => s.UpdateUserCurrency(1, cos!.CurrencyId, cos.Price), Times.Once);

        Assert.Equal(0, await context.UserCosmetics.CountAsync());
    }
}
