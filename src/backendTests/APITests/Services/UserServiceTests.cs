/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace APITests.Services;

public class UserServiceTests
{
    private readonly UserService _svcMockedHandler;
    private readonly AppDbContext _context;
    private readonly Mock<IUserRoleStore<User>> _userStore;
    private readonly UserService _svc;
    private readonly Mock<IRoleStore<ApplicationRole>> _roleStore;
    private readonly UserService _svcMockedUserManager;
    private readonly Mock<UserManager<User>> _mockUserManager;

    public UserServiceTests()
    {
        var (context, _) = CreateSqliteContext();

        var (userManager, userStore) = CreateUserManagerMock(context);
        var (roleManager, roleStore) = CreateRoleManagerMock();
        var userManagerMock = CreateUserManagerMock();

        this._context = context;
        this._userStore = userStore;
        this._roleStore = roleStore;
        this._mockUserManager = userManagerMock;

        _svcMockedHandler = new UserService(
            context,
            userManager,
            roleManager,
            userHandler: Mock.Of<UserHandler>(),
            currencyService: Mock.Of<ICurrencyService>(),
            userCurrencyHandler: Mock.Of<UserCurrencyHandler>(),
            cosmeticService: Mock.Of<ICosmeticService>(),
            userCosmeticHandler: Mock.Of<UserCosmeticHandler>(),
            characterService: Mock.Of<ICharacterService>(),
            userCharacterHandler: Mock.Of<UserCharacterHandler>(),
            topicService: Mock.Of<ITopicService>()
        );

        _svcMockedUserManager = new UserService(
            context,
            _mockUserManager.Object,
            roleManager,
            userHandler: new UserHandler(),
            currencyService: Mock.Of<ICurrencyService>(),
            userCurrencyHandler: Mock.Of<UserCurrencyHandler>(),
            cosmeticService: Mock.Of<ICosmeticService>(),
            userCosmeticHandler: Mock.Of<UserCosmeticHandler>(),
            characterService: Mock.Of<ICharacterService>(),
            userCharacterHandler: Mock.Of<UserCharacterHandler>(),
            topicService: Mock.Of<ITopicService>()
        );

        _svc = new UserService(
            context,
            userManager,
            roleManager,
            userHandler: new UserHandler(),
            currencyService: Mock.Of<ICurrencyService>(),
            userCurrencyHandler: Mock.Of<UserCurrencyHandler>(),
            cosmeticService: Mock.Of<ICosmeticService>(),
            userCosmeticHandler: Mock.Of<UserCosmeticHandler>(),
            characterService: Mock.Of<ICharacterService>(),
            userCharacterHandler: Mock.Of<UserCharacterHandler>(),
            topicService: Mock.Of<ITopicService>()
        );
    }
    private static (AppDbContext Ctx, SqliteConnection Conn) CreateSqliteContext()
    {
        var conn = new SqliteConnection("Filename=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;

        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();

        // Seed users (used by VerifyUserExists/GetUsers and also by UserManager.Users IQueryable)
        ctx.Users.AddRange(
            new User { Id = 1, FirstName = "John", LastName = "Pork", Email = "john@test.com", UserName = "john@test.com", DisplayName = "JohnPork" },
            new User { Id = 2, FirstName = "Sara", LastName = "Pork", Email = "sara@test.com", UserName = "sara@test.com", DisplayName = "SaraPork" }
        );

        ctx.Currencies.Add(
            new Currency { Id = 2, Name = "Gold", Sprite = "Gold.png", StartingAmount = 5 });

        ctx.Cosmetics.AddRange(
            new Cosmetic
            {
                Id = 10,
                Name = "Default Hat",
                ClothingType = ClothingType.Hat,
                DefaultOwned = true,
                Price = 0,
                CurrencyId = 2,
                RiveFile = "test.riv",
                RiveArtboard = "Board",
                RiveStateMachine = "State",
                RiveInput = "Input",
                IconFile = "icon.jpg"
            },
            new Cosmetic
            {
                Id = 11,
                Name = "Not Default Shirt",
                ClothingType = ClothingType.Shirt,
                DefaultOwned = false,
                Price = 25,
                CurrencyId = 2,
                RiveFile = "test.riv",
                RiveArtboard = "Board",
                RiveStateMachine = "State",
                RiveInput = "Input",
                IconFile = "icon.jpg"
            }
        );

        ctx.Characters.AddRange(
            new Character { Id = 1, Name = "dog" },
            new Character { Id = 2, Name = "cat" }
        );

        ctx.UserStreaks.AddRange(
            new UserStreak { UserId = 1, CurrentStreak = 5, HighestStreak = 10 },
            new UserStreak { UserId = 2, CurrentStreak = 3, HighestStreak = 8 }
        );

        ctx.SaveChanges();
        return (ctx, conn);
    }
    public class TestPasswordHasher : IPasswordHasher<User>
    {
        public string HashPassword(User user, string password)
            => "FAKE_HASH";

        public PasswordVerificationResult VerifyHashedPassword(
            User user,
            string hashedPassword,
            string providedPassword)
            => PasswordVerificationResult.Success;
    }
    private static (UserManager<User>, Mock<IUserRoleStore<User>>) CreateUserManagerMock(AppDbContext context)
    {

        var userStore = new Mock<IUserRoleStore<User>>();
        userStore.As<IQueryableUserStore<User>>()
            .Setup(s => s.Users)
            .Returns(context.Users);
        userStore.As<IUserPasswordStore<User>>();

        var userManager = new UserManager<User>(userStore.Object, null!, new TestPasswordHasher(), null!, null!, null!, null!, null!, null!);
        return (userManager, userStore);
    }

    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserClaimStore<User>>();

        var um = new Mock<UserManager<User>>(
            store.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<User>>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<User>>>()
        );

        return um;
    }

    private static (RoleManager<ApplicationRole>, Mock<IRoleStore<ApplicationRole>>) CreateRoleManagerMock()
    {
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();

        var roleManager = new RoleManager<ApplicationRole>(roleStore.Object, null!, null!, null!, null!);
        return (roleManager, roleStore);
    }

    [Fact(DisplayName = "VerifyUserExists: completes when user exists")]
    public async Task VerifyUserExists_Completes_WhenExists()
    {
        await _svcMockedHandler.VerifyUserExists(1);
    }

    [Fact(DisplayName = "VerifyUserExists: throws when user does not exist")]
    public async Task VerifyUserExists_Throws_WhenMissing()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => _svcMockedHandler.VerifyUserExists(999));
        Assert.Equal("User not found", ex.Message);
    }

    [Fact(DisplayName = "GetUser: returns when UserManager finds user")]
    public async Task GetUser_Returns_WhenFound()
    {
        var user = await _context.Users.FindAsync(1);
        _userStore.Setup(m => m.FindByIdAsync("1", CancellationToken.None)).ReturnsAsync(user);

        var result = await _svcMockedHandler.GetUser(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact(DisplayName = "GetUser: throws when UserManager does not find user")]
    public async Task Throws_WhenUserNotFound()
    {
        var user = await _context.Users.FindAsync(1);
        _userStore.Setup(m => m.FindByIdAsync("1", CancellationToken.None)).ReturnsAsync(user);

        var ex = await Assert.ThrowsAsync<Exception>(() => _svcMockedHandler.GetUser(999));
        Assert.Equal("User not found", ex.Message);
    }

    [Fact(DisplayName = "GetUsers: returns paged users")]
    public async Task GetUsers_ReturnsPaged()
    {
        var page1 = await _svcMockedHandler.GetUsers(range: 1, offset: 0);

        Assert.Single(page1);
        Assert.Equal(1, page1.First().Id);

        var page2 = await _svcMockedHandler.GetUsers(range: 1, offset: 1);

        Assert.Single(page2);
        Assert.Equal(2, page2.First().Id);
    }

    [Fact(DisplayName = "GetUsers: throws when no users found in page")]
    public async Task GetUsers_Throws_WhenNone()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => _svcMockedHandler.GetUsers(range: 10, offset: 999));
        Assert.Equal("No users found", ex.Message);
    }

    [Fact(DisplayName = "GetUserRole: returns first role when present")]
    public async Task GetUserRole_ReturnsFirstRole_WhenPresent()
    {

        var user = await _context.Users.FindAsync(1);

        _userStore
            .Setup(s => s.GetRolesAsync(user!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(["Admin", "User"]);

        string role = await _svcMockedHandler.GetUserRole(user!);

        Assert.Equal("Admin", role);
    }

    [Fact(DisplayName = "GetUserRole: returns default role when absent")]
    public async Task GetUserRole_ReturnsDefault_WhenNone()
    {
        var user = await _context.Users.FindAsync(1);

        _userStore
            .Setup(s => s.GetRolesAsync(user!, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        string role = await _svcMockedHandler.GetUserRole(user!);

        Assert.Equal(Roles.User, role);
    }

    [Fact(DisplayName = "CreateUser: throws on duplicate email")]
    public async Task CreateUser_ThrowOnDuplicateEmail()
    {
        UserCreateRecord ucr = new UserCreateRecord() { Email = "john@test.com" };

        await Assert.ThrowsAsync<BadHttpRequestException>(() => _svcMockedHandler.CreateUser(ucr));
        Assert.Empty(await _context.UserCurrencies.ToListAsync());
    }

    [Fact(DisplayName = "CreateUser: throws when CreateAsync fails (uses first error description)")]
    public async Task CreateUser_Throws_WhenCreateFails()
    {
        var ucr = new UserCreateRecord()
        {
            Email = "new@test.com",
            Password = "P@ssw0rd!",
            FirstName = "New",
            LastName = "User",
            Role = "User"
        };

        _userStore.Setup(s => s.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Create failed" }));

        var ex = await Assert.ThrowsAsync<Exception>(async () => await _svc.CreateUser(ucr));

        Assert.Empty(await _context.UserCurrencies.ToListAsync());
        Assert.Empty(await _context.UserCosmetics.ToListAsync());
    }

    [Fact(DisplayName = "CreateUser: throws when roles does not exist")]
    public async Task CreateUser_Throws_WhenRoleDoesntExist()
    {
        _userStore
            .Setup(s => s.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        _roleStore
            .Setup(s => s.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationRole?)null);

        var ucr = new UserCreateRecord()
        {
            Email = "new@test.com",
            Password = "P@ssw0rd!",
            DisplayName = "Bong",
            FirstName = "New",
            LastName = "User",
            Role = "Noob"
        };

        var ex = await Assert.ThrowsAsync<Exception>(async () => await _svc.CreateUser(ucr));

        Assert.Empty(await _context.UserCurrencies.ToListAsync());
        Assert.Empty(await _context.UserCosmetics.ToListAsync());
        Assert.Equal("Role 'Noob' does not exist.", ex.Message);
    }

    [Fact(DisplayName = "CreateUser: throws when role can not be added")]
    public async Task CreateUser_Throws_WhenRoleNotAdded()
    {
        _userStore
            .Setup(s => s.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        _userStore
            .Setup(s => s.UpdateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Update failed" }));

        _roleStore
            .Setup(s => s.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationRole { Name = Roles.User });

        var ucr = new UserCreateRecord()
        {
            Email = "new@test.com",
            Password = "P@ssw0rd!",
            DisplayName = "Bong",
            FirstName = "New",
            LastName = "User",
            Role = Roles.User
        };

        var ex = await Assert.ThrowsAsync<Exception>(async () => await _svc.CreateUser(ucr));

        Assert.Empty(await _context.UserCurrencies.ToListAsync());
        Assert.Empty(await _context.UserCosmetics.ToListAsync());
        Assert.Equal("Could not add role 'User' to user", ex.Message);
    }

    [Fact(DisplayName = "CreateUser: Success")]
    public async Task CreateUser_Success()
    {
        var (context, conn) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = conn;

        var (userManager, userStore) = CreateUserManagerMock(context);
        var (roleManager, roleStore) = CreateRoleManagerMock();

        userStore
            .Setup(s => s.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) =>
            {
                context.Users.Add(u);
                context.SaveChanges();
                return IdentityResult.Success;
            });

        userStore
            .Setup(s => s.UpdateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        userStore
            .Setup(s => s.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        userStore
            .Setup(s => s.GetRolesAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        userStore
            .Setup(s => s.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        roleStore
            .Setup(s => s.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationRole { Name = Roles.User });

        var ucr = new UserCreateRecord()
        {
            Email = "new@test.com",
            Password = "P@ssw0rd!",
            DisplayName = "Bong",
            FirstName = "New",
            LastName = "User",
            Role = Roles.User
        };

        UserHandler userHandler = new();

        var topicService = new Mock<ITopicService>();
        topicService.Setup(s => s.DefaultUserProficiencies(It.IsAny<int>())).Returns(Task.CompletedTask);

        var currencyService = new Mock<ICurrencyService>();
        currencyService
            .Setup(s => s.GetCurrencies())
            .ReturnsAsync(await context.Currencies.ToListAsync());

        var cosmeticService = new Mock<ICosmeticService>();
        cosmeticService
            .Setup(s => s.GetDefaultCosmetics())
            .ReturnsAsync(await context.Cosmetics.ToListAsync());

        var characterService = new Mock<ICharacterService>();
        characterService
            .Setup(s => s.GetDefaultCharacters())
            .ReturnsAsync(await context.Characters.ToListAsync());

        var svc = new UserService(
            context,
            userManager,
            roleManager,
            userHandler,
            currencyService.Object,
            Mock.Of<UserCurrencyHandler>(),
            cosmeticService.Object,
            Mock.Of<UserCosmeticHandler>(),
            characterService.Object,
            Mock.Of<UserCharacterHandler>(),
            topicService.Object
        );

        var user = await svc.CreateUser(ucr);
        Assert.NotNull(user);
        Assert.NotEmpty(await context.UserCurrencies.ToListAsync());
        Assert.NotEmpty(await context.UserCosmetics.ToListAsync());
        Assert.NotEmpty(await context.Characters.ToListAsync());
        Assert.NotNull(await context.UserStreaks.FindAsync(user.Id));
    }

    [Fact(DisplayName = "UpdateUser: throws when role can not be added")]
    public async Task UpdateUser_Throws_WhenRoleNotAdded()
    {
        var user = await _context.Users.FindAsync(1);
        _userStore.Setup(m => m.FindByIdAsync("1", CancellationToken.None)).ReturnsAsync(user);

        var ucr = new UserCreateRecord()
        {
            Email = "new@test.com",
            Password = "P@ssw0rd!",
            DisplayName = "Bong",
            FirstName = "New",
            LastName = "User",
            Role = "Noob"
        };

        var ex = await Assert.ThrowsAsync<Exception>(async () => await _svc.UpdateUser(1, ucr));

        Assert.Empty(await _context.UserCurrencies.ToListAsync());
        Assert.Empty(await _context.UserCosmetics.ToListAsync());
        Assert.Equal("Role 'Noob' does not exist.", ex.Message);
    }
    [Fact(DisplayName = "UpdateUser: throws when UpdateAsync fails (uses first error description)")]
    public async Task UpdateUser_Throws_WhenUpdateAsyncFails()
    {
        var user = await _context.Users.FindAsync(1);

        _userStore
            .Setup(s => s.FindByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _roleStore
            .Setup(s => s.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationRole { Name = Roles.User });

        _userStore
            .Setup(s => s.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

        var ucr = new UserCreateRecord()
        {
            Email = "john@test.com",
            Password = "",
            FirstName = "John",
            LastName = "Pork",
            Role = Roles.User
        };

        var ex = await Assert.ThrowsAsync<Exception>(async () => await _svc.UpdateUser(1, ucr));
        Assert.Equal("Update failed", ex.Message);
    }

    [Fact(DisplayName = "UpdateUser: throws when password reset fails (uses first error description)")]
    public async Task UpdateUser_Throws_WhenPasswordResetFails()
    {
        var user = await _context.Users.FindAsync(1);

        _userStore
            .Setup(s => s.FindByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _roleStore
            .Setup(s => s.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationRole { Name = Roles.User });

        _userStore
            .Setup(s => s.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        _userStore.As<IUserPasswordStore<User>>()
            .Setup(s => s.SetPasswordHashAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var updateCallCount = 0;
        _userStore
            .Setup(s => s.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                updateCallCount++;
                return updateCallCount == 1
                    ? IdentityResult.Success
                    : IdentityResult.Failed(new IdentityError { Description = "Password reset failed" });
            });

        var ucr = new UserCreateRecord()
        {
            Email = "john@test.com",
            Password = "NewP@ssw0rd!",
            FirstName = "John",
            LastName = "Pork",
            Role = Roles.User
        };

        var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _svc.UpdateUser(1, ucr));
    }

    [Fact(DisplayName = "UpdateUser: throws when RemoveFromRolesAsync fails (uses first error description)")]
    public async Task UpdateUser_Throws_WhenRemoveRolesFails()
    {
        var user = await _context.Users.FindAsync(1);

        _userStore
            .Setup(s => s.FindByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _roleStore
            .Setup(s => s.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationRole { Name = Roles.User });

        _userStore
            .Setup(s => s.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        _userStore
            .Setup(s => s.GetRolesAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "Admin" });

        var updateCallCount = 0;
        _userStore
            .Setup(s => s.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                updateCallCount++;
                return updateCallCount == 1
                    ? IdentityResult.Success
                    : IdentityResult.Failed(new IdentityError { Description = "Remove roles failed" });
            });

        _userStore
            .Setup(s => s.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var ucr = new UserCreateRecord()
        {
            Email = "john@test.com",
            Password = "", // no password change
            FirstName = "John",
            LastName = "Pork",
            Role = Roles.User
        };

        var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _svc.UpdateUser(1, ucr));
    }

    [Fact(DisplayName = "UpdateUser: throws when AddToRoleAsync fails (uses first error description)")]
    public async Task UpdateUser_Throws_WhenAddRoleFails()
    {
        var user = await _context.Users.FindAsync(1);

        _userStore
            .Setup(s => s.FindByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _roleStore
            .Setup(s => s.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationRole { Name = Roles.User });

        _userStore
            .Setup(s => s.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        _userStore
            .Setup(s => s.GetRolesAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        var updateCallCount = 0;
        _userStore
            .Setup(s => s.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                updateCallCount++;
                return updateCallCount < 2
                    ? IdentityResult.Success
                    : IdentityResult.Failed(new IdentityError { Description = "Add role failed" });
            });

        _userStore
            .Setup(s => s.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var ucr = new UserCreateRecord()
        {
            Email = "john@test.com",
            Password = "", // no password change
            FirstName = "John",
            LastName = "Pork",
            Role = Roles.User
        };

        var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await _svc.UpdateUser(1, ucr));
    }

    [Fact(DisplayName = "DeleteUser: deletes user when user has no roles")]
    public async Task DeleteUser_Deletes_WhenNoRoles()
    {
        var user = await _context.Users.FindAsync(1);

        _userStore
            .Setup(s => s.FindByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userStore
            .Setup(s => s.GetRolesAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        _userStore
            .Setup(s => s.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);

        await _svc.DeleteUser(1);

        _userStore.Verify(s => s.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DeleteUser: throws when DeleteAsync fails (uses first error description)")]
    public async Task DeleteUser_Throws_WhenDeleteFails()
    {
        var user = await _context.Users.FindAsync(1);

        _userStore
            .Setup(s => s.FindByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userStore
            .Setup(s => s.GetRolesAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        _userStore
            .Setup(s => s.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Delete failed" }));

        var ex = await Assert.ThrowsAsync<Exception>(async () => await _svc.DeleteUser(1));
        Assert.Equal("Delete failed", ex.Message);
    }

    [Fact]
    public async Task AddClaimAsync_UserHasNoClaims_AddsClaim()
    {
        User user = new User { Id = 1, Email = "john@test.com", UserName = "john@test.com" };

        _mockUserManager
            .Setup(m => m.GetClaimsAsync(user))
            .ReturnsAsync(new List<Claim>());

        _mockUserManager
            .Setup(m => m.AddClaimAsync(user, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _svcMockedUserManager.AddClaimAsync(user, ClaimTypes.Email, "john@test.com");

        // Assert
        _mockUserManager.Verify(
            m => m.AddClaimAsync(user, It.Is<Claim>(c =>
            c.Type == ClaimTypes.Email && c.Value == "john@test.com")),
            Times.Once());
    }

    [Fact]
    public async Task AddClaimAsync_UserHasOtherClaims_AddsClaim()
    {
        User user = new User { Id = 1, Email = "john@test.com", UserName = "john@test.com" };

        var existingClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "user@example.com")
            };

        _mockUserManager
            .Setup(m => m.GetClaimsAsync(user))
            .ReturnsAsync(existingClaims);

        _mockUserManager
            .Setup(m => m.AddClaimAsync(user, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _svcMockedUserManager.AddClaimAsync(user, ClaimTypes.Role, "Admin");

        // Assert
        _mockUserManager.Verify(
            m => m.AddClaimAsync(user, It.Is<Claim>(c =>
                c.Type == ClaimTypes.Role && c.Value == "Admin")),
            Times.Once);
    }

    [Fact]
    public async Task AddClaimAsync_UserAlreadyHasSameClaimType_DoesNotAddClaim()
    {
        User user = new User { Id = 1, Email = "john@test.com", UserName = "john@test.com" };

        var existingClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Admin")
            };

        _mockUserManager
            .Setup(m => m.GetClaimsAsync(user))
            .ReturnsAsync(existingClaims);

        // Act
        await _svcMockedUserManager.AddClaimAsync(user, ClaimTypes.Role, "Admin");

        // Assert
        _mockUserManager.Verify(
            m => m.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()),
            Times.Never);
    }

    [Fact]
    public async Task AddClaimAsync_UserAlreadyHasSameClaimTypeWithDifferentValue_DoesAddClaim()
    {
        User user = new User { Id = 1, Email = "john@test.com", UserName = "john@test.com" };

        var existingClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "User") // same type, different value
            };

        _mockUserManager
            .Setup(m => m.GetClaimsAsync(user))
            .ReturnsAsync(existingClaims);

        // Act
        await _svcMockedUserManager.AddClaimAsync(user, ClaimTypes.Role, "Admin");

        // Assert
        _mockUserManager.Verify(
            m => m.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()),
            Times.Once);
    }

    [Fact]
    public async Task AddClaimAsync_GetClaimsThrows_ThrowsExpectedException()
    {
        var userManager = CreateUserManagerMock();

        User user = new User { Id = 1, Email = "john@test.com", UserName = "john@test.com" };

        userManager
            .Setup(m => m.GetClaimsAsync(user))
            .ThrowsAsync(new Exception("DB error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => _svc.AddClaimAsync(user, ClaimTypes.Role, "Admin"));

        Assert.Equal("Failed to add claim to user", ex.Message);
    }

    [Fact]
    public async Task AddClaimAsync_AddClaimThrows_ThrowsExpectedException()
    {
        var userManager = CreateUserManagerMock();

        User user = new User { Id = 1, Email = "john@test.com", UserName = "john@test.com" };

        userManager
            .Setup(m => m.GetClaimsAsync(user))
            .ReturnsAsync(new List<Claim>());

        userManager
            .Setup(m => m.AddClaimAsync(user, It.IsAny<Claim>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => _svc.AddClaimAsync(user, ClaimTypes.Role, "Admin"));

        Assert.Equal("Failed to add claim to user", ex.Message);
    }

    [Fact(DisplayName = "GetUserByName: returns user when found")]
    public async Task GetUserByName_Returns_WhenFound()
    {
        _userStore
            .Setup(s => s.FindByNameAsync("john@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(await _context.Users.FindAsync(1));

        var result = await _svcMockedHandler.GetUserByName("john@test.com");

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("john@test.com", result.UserName);
    }

    [Fact(DisplayName = "GetUserByName: throws when user not found")]
    public async Task GetUserByName_Throws_WhenNotFound()
    {
        _userStore
            .Setup(s => s.FindByNameAsync("nonexistent@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _svcMockedHandler.GetUserByName("nonexistent@test.com"));
        Assert.Equal("No user found with this username", ex.Message);
    }

    [Fact(DisplayName = "GetUserByName: returns correct user when multiple users exist")]
    public async Task GetUserByName_ReturnsCorrectUser_WhenMultipleUsersExist()
    {
        _userStore
            .Setup(s => s.FindByNameAsync("sara@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(await _context.Users.FindAsync(2));

        var result = await _svcMockedHandler.GetUserByName("sara@test.com");

        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("SaraPork", result.DisplayName);
    }

    [Fact]
    public async Task GetStreak()
    {
        // for reference because this test suite is way too big...
        //ctx.UserStreaks.AddRange(
        //    new UserStreak { UserId = 1, CurrentStreak = 5, HighestStreak = 10 },
        //    new UserStreak { UserId = 2, CurrentStreak = 3, HighestStreak = 8 }
        //);

        var result = await _svcMockedHandler.GetUserStreak(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal(5, result.CurrentStreak);
        Assert.Equal(10, result.HighestStreak);

        result = await _svcMockedHandler.GetUserStreak(2);
        Assert.NotNull(result);
        Assert.Equal(2, result.UserId);
        Assert.Equal(3, result.CurrentStreak);
        Assert.Equal(8, result.HighestStreak);
    }

    [Fact]
    public async Task CreateUser_HandlesDuplicateFieldsCorrectly()
    {
        _context.Users.AddRange(CreateUser(4), CreateUser(5), CreateUser(6));
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<BadHttpRequestException>(() => _svc.ThrowUserDuplicateProperty(
            UserCreateRecord("test@test.com4", "ABC")
        ));

        await Assert.ThrowsAsync<BadHttpRequestException>(() => _svc.CreateUser(
            UserCreateRecord("test@test.com4", "JohnDoe6")
        ));

        // Should not throw 
        await _svc.ThrowUserDuplicateProperty(
            UserCreateRecord("test@test.com7", "JohnDoe7")
        );
    }

    private UserCreateRecord UserCreateRecord(string email, string displayName)
    {
        return new UserCreateRecord
        {
            FirstName = "A",
            LastName = "B",
            DisplayName = displayName,
            Email = email,
            Password = "Test123!",
            Role = "User"
        };
    }

    private User CreateUser(int id)
    {
        return new User
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "JohnDoe" + id,
            UserName = "testuser",
            NormalizedUserName = "TESTUSER" + id,
            Email = "test@test.com" + id,
            NormalizedEmail = "TEST@TEST.COM" + id,
            SecurityStamp = Guid.NewGuid().ToString()
        };
    }
}