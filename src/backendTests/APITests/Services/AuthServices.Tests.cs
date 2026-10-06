/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using API.Handlers.GameEventHandlers;
using Moq;

using Models;
using API.Services;


namespace APITests.Services;

public class AuthServiceTests
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

    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();

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

    private static Mock<SignInManager<User>> CreateSignInManagerMock(UserManager<User> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        contextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
        var options = Mock.Of<IOptions<IdentityOptions>>();
        var logger = Mock.Of<ILogger<SignInManager<User>>>();
        var schemes = Mock.Of<IAuthenticationSchemeProvider>();
        var confirmation = Mock.Of<IUserConfirmation<User>>();

        var sim = new Mock<SignInManager<User>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            options,
            logger,
            schemes,
            confirmation
        );

        return sim;
    }

    [Fact(DisplayName = "RegisterUser: calls CreateUser then SignInAsync")]
    public async Task RegisterUser_Success_CallsCreateUser_AndSignIn()
    {
        var (context, conn) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = conn;

        var userService = new Mock<IUserService>();
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);

        var ucr = new UserCreateRecord()
        {
            Email = "new@test.com",
            Password = "P@ssw0rd!",
            FirstName = "New",
            LastName = "User",
            Role = Roles.User
        };

        var createdUser = new User { Id = 123, Email = ucr.Email, UserName = ucr.Email };

        userService
            .Setup(s => s.CreateUser(ucr))
            .ReturnsAsync(createdUser);

        userManager
            .Setup(m => m.ResetAccessFailedCountAsync(createdUser))
            .ReturnsAsync(IdentityResult.Success);

        userManager
            .Setup(m => m.GetClaimsAsync(createdUser))
            .ReturnsAsync(new List<Claim>());
        userManager
            .Setup(m => m.AddClaimAsync(createdUser, new Claim(ClaimTypes.NameIdentifier, "1")))
            .ReturnsAsync(IdentityResult.Success);


        signInManager
            .Setup(m => m.SignInAsync(createdUser, false, null))
            .Returns(Task.CompletedTask);

        var svc = new AuthService(context, userService.Object, userManager.Object, signInManager.Object, Mock.Of<IGameEventManager>());

        await svc.RegisterUser(ucr);

        userService.Verify(s => s.CreateUser(ucr), Times.Once);
        userManager.Verify(m => m.ResetAccessFailedCountAsync(createdUser), Times.Once);
        signInManager.Verify(m => m.SignInAsync(createdUser, false, null), Times.Once);
    }

    [Fact(DisplayName = "RegisterUser: rethrows when CreateUser fails and does not sign in")]
    public async Task RegisterUser_Throws_WhenCreateUserFails()
    {
        var (context, conn) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = conn;

        var userService = new Mock<IUserService>();
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);

        var ucr = new UserCreateRecord()
        {
            Email = "new@test.com",
            Password = "P@ssw0rd!",
            Role = Roles.User
        };

        userService
            .Setup(s => s.CreateUser(ucr))
            .ThrowsAsync(new Exception("Create failed"));

        var svc = new AuthService(context, userService.Object, userManager.Object, signInManager.Object, Mock.Of<IGameEventManager>());

        var ex = await Assert.ThrowsAsync<Exception>(() => svc.RegisterUser(ucr));
        Assert.Equal("Create failed", ex.Message);

        signInManager.Verify(m => m.SignInAsync(It.IsAny<User>(), It.IsAny<bool>(), null), Times.Never);
        userManager.Verify(m => m.ResetAccessFailedCountAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact(DisplayName = "AuthenticateUserAsync: throws when user does not exist")]
    public async Task AuthenticateUser_Throws_WhenUserNotFound()
    {
        var (context, conn) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = conn;

        var userService = new Mock<IUserService>();
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);

        userManager
            .Setup(m => m.FindByEmailAsync("missing@test.com"))
            .ReturnsAsync((User?)null);

        var svc = new AuthService(context, userService.Object, userManager.Object, signInManager.Object, Mock.Of<IGameEventManager>());

        var ex = await Assert.ThrowsAsync<Exception>(() => svc.AuthenticateUserAsync("missing@test.com", "pw"));
        Assert.Equal("User does not exist.", ex.Message);
    }

    [Fact(DisplayName = "AuthenticateUserAsync: throws when account locked")]
    public async Task AuthenticateUser_Throws_WhenLockedOut()
    {
        var (context, conn) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = conn;

        var userService = new Mock<IUserService>();
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);

        var user = new User { Id = 1, Email = "john@test.com", UserName = "john@test.com" };

        userManager
            .Setup(m => m.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        userManager
            .Setup(m => m.IsLockedOutAsync(user))
            .ReturnsAsync(true);

        var svc = new AuthService(context, userService.Object, userManager.Object, signInManager.Object, Mock.Of<IGameEventManager>());

        var ex = await Assert.ThrowsAsync<Exception>(() => svc.AuthenticateUserAsync(user.Email!, "pw"));
        Assert.Equal("Account locked.", ex.Message);
    }

    [Fact(DisplayName = "AuthenticateUserAsync: invalid credentials calls AccessFailedAsync then throws")]
    public async Task AuthenticateUser_Throws_WhenInvalidCredentials()
    {
        var (context, conn) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = conn;

        var userService = new Mock<IUserService>();
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);

        var user = new User { Id = 1, Email = "john@test.com", UserName = "john@test.com" };

        userManager
            .Setup(m => m.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        userManager
            .Setup(m => m.IsLockedOutAsync(user))
            .ReturnsAsync(false);

        userManager
            .Setup(m => m.CheckPasswordAsync(user, "badpw"))
            .ReturnsAsync(false);

        userManager
            .Setup(m => m.AccessFailedAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var svc = new AuthService(context, userService.Object, userManager.Object, signInManager.Object, Mock.Of<IGameEventManager>());

        var ex = await Assert.ThrowsAsync<Exception>(() => svc.AuthenticateUserAsync(user.Email!, "badpw"));
        Assert.Equal("Invalid credentials.", ex.Message);

        userManager.Verify(m => m.AccessFailedAsync(user), Times.Once);
    }

    [Fact(DisplayName = "AuthenticateUserAsync: returns user when valid credentials")]
    public async Task AuthenticateUser_ReturnsUser_WhenValid()
    {
        var (context, conn) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = conn;

        var userService = new Mock<IUserService>();
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);

        var user = new User { Id = 1, Email = "john@test.com", UserName = "john@test.com" };

        userManager
            .Setup(m => m.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        userManager
            .Setup(m => m.IsLockedOutAsync(user))
            .ReturnsAsync(false);

        userManager
            .Setup(m => m.CheckPasswordAsync(user, "goodpw"))
            .ReturnsAsync(true);

        var svc = new AuthService(context, userService.Object, userManager.Object, signInManager.Object, Mock.Of<IGameEventManager>());

        var result = await svc.AuthenticateUserAsync(user.Email!, "goodpw");

        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);

        userManager.Verify(m => m.AccessFailedAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact(DisplayName = "SignInAsync: resets failed count and signs in")]
    public async Task SignInAsync_Success_ResetsAndSignsIn()
    {
        var (context, conn) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = conn;

        var userService = new Mock<IUserService>();
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);

        var user = new User { Id = 1, Email = "john@test.com", UserName = "john@test.com" };

        userManager
            .Setup(m => m.ResetAccessFailedCountAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        userManager
            .Setup(m => m.GetClaimsAsync(user))
            .ReturnsAsync(new List<Claim>());
        userManager
            .Setup(m => m.AddClaimAsync(user, new Claim(ClaimTypes.NameIdentifier, "1")))
            .ReturnsAsync(IdentityResult.Success);


        signInManager
            .Setup(m => m.SignInAsync(user, true, null))
            .Returns(Task.CompletedTask);

        var svc = new AuthService(context, userService.Object, userManager.Object, signInManager.Object, Mock.Of<IGameEventManager>());

        await svc.SignInAsync(user, isPersistent: true);

        userManager.Verify(m => m.ResetAccessFailedCountAsync(user), Times.Once);
        signInManager.Verify(m => m.SignInAsync(user, true, null), Times.Once);
    }

    [Fact(DisplayName = "SignInAsync: throws generic message when sign-in fails")]
    public async Task SignInAsync_Throws_Generic_WhenFails()
    {
        var (context, conn) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = conn;

        var userService = new Mock<IUserService>();
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);

        var user = new User { Id = 1, Email = "john@test.com", UserName = "john@test.com" };

        userManager
            .Setup(m => m.ResetAccessFailedCountAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        signInManager
            .Setup(m => m.SignInAsync(user, false, null))
            .ThrowsAsync(new Exception("boom"));

        var svc = new AuthService(context, userService.Object, userManager.Object, signInManager.Object, Mock.Of<IGameEventManager>());

        var ex = await Assert.ThrowsAsync<Exception>(() => svc.SignInAsync(user, isPersistent: false));
        Assert.Equal("Failed to sign in user.", ex.Message);
    }

    [Fact(DisplayName = "SignOutAsync: calls SignInManager.SignOutAsync")]
    public async Task SignOutAsync_CallsSignOut()
    {
        var (context, conn) = CreateSqliteContext();
        await using var _ = context;
        await using var __ = conn;

        var userService = new Mock<IUserService>();
        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);

        signInManager
            .Setup(m => m.SignOutAsync())
            .Returns(Task.CompletedTask);

        var svc = new AuthService(context, userService.Object, userManager.Object, signInManager.Object, Mock.Of<IGameEventManager>());

        await svc.SignOutAsync();

        signInManager.Verify(m => m.SignOutAsync(), Times.Once);
    }

}
