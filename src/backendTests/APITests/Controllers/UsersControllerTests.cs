/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */


using System.Security.Claims;
using API.Controllers;
using API.Controllers.DTOs;
using Models;
using API.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.AspNetCore.Http;


namespace APITests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockService = new Mock<IUserService>();
        _controller = new UsersController(_mockService.Object);
    }

    private static User DummyUser()
    {
        return new User
        {
            Id = 1
        };
    }


    private static ICollection<User> DummyUsers()
    {
        return
        [
            new User {
                Id = 1
            },
            new User {
                Id = 2
            },
            new User {
                Id = 3
            },
            new User {
                Id = 4
            },
            new User {
                Id = 5
            }
        ];
    }

    private static UserCreateRecord DummyUserCreateRecord()
    {
        return UserCreateDto.CreateRecordFromDto(DummyUserCreateDto());
    }

    private static UserCreateDto DummyUserCreateDto()
    {
        return new UserCreateDto()
        {
            FirstName = "John",
            LastName = "Pork",
            DisplayName = "JohnPork",
            Password = "Porking234!",
            Email = "JohnPork@gmail.com"
        };
    }

    private void SetAuthenticatedUser(int userId, bool isAdmin = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, Roles.Admin));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }


    [Fact]
    public async Task GetUserReturnsUser()
    {
        // setup
        var returnedUser = DummyUser();
        var returnedUserDto = UserDto.CreateUserDto(returnedUser, null);
        int id = returnedUser.Id;
        SetAuthenticatedUser(id);
        _mockService.Setup(s => s.GetUser(id)).ReturnsAsync(returnedUser);

        // act
        var result = await _controller.GetUser(id);

        // assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var sentUser = Assert.IsAssignableFrom<UserDto>(okObjectResult.Value);
        Assert.Equal(sentUser.Id, returnedUserDto.Id);
    }

    [Fact]
    public async Task GetUserReturns404OnNonExistingUser()
    {
        // setup
        const int id = -1;
        SetAuthenticatedUser(id);
        _mockService.Setup(s => s.GetUser(id)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.GetUser(id);

        // assert
        var res = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, res.StatusCode);
    }

    [Fact]
    public async Task GetUserReturnsForbidForOtherUserWhenNotAdmin()
    {
        // setup
        const int id = 1;
        SetAuthenticatedUser(999);

        // act
        var result = await _controller.GetUser(id);

        // assert
        Assert.IsType<ForbidResult>(result.Result);
        _mockService.Verify(s => s.GetUser(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetUserReturnsUserForOtherUserWhenAdmin()
    {
        // setup
        var returnedUser = DummyUser();
        int id = returnedUser.Id;
        SetAuthenticatedUser(999, isAdmin: true);
        _mockService.Setup(s => s.GetUser(id)).ReturnsAsync(returnedUser);

        // act
        var result = await _controller.GetUser(id);

        // assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var sentUser = Assert.IsAssignableFrom<UserDto>(okObjectResult.Value);
        Assert.Equal(id, sentUser.Id);
    }

    [Fact]
    public async Task GetUsersReturnsUsers()
    {
        // setup
        const int range = 5;
        const int offset = 0;
        var returnedUsers = DummyUsers();
        _mockService.Setup(s => s.GetUsers(range, offset)).ReturnsAsync(returnedUsers);

        // act
        var result = await _controller.GetUsers(offset, range);

        // assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var sentUsers = Assert.IsAssignableFrom<ICollection<UserDto>>(okObjectResult.Value);
        Assert.Equal(sentUsers.Count, returnedUsers.Count);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, -1)]
    [InlineData(0, -1)]
    public async Task GetUsersReturns400OnInvalidInput(int range, int offset)
    {
        // act
        var result = await _controller.GetUsers(offset, range);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetUsersReturns500OnFail()
    {
        // setup
        const int range = 5;
        const int offset = 0;
        _mockService.Setup(s => s.GetUsers(range, offset)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.GetUsers(range, offset);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task CreateUserCallsCreateUser()
    {
        // setup
        var receivedRecord = DummyUserCreateRecord();
        var receivedDto = DummyUserCreateDto();
        var returnedUser = DummyUser();
        _mockService.Setup(s => s.CreateUser(receivedRecord)).ReturnsAsync(returnedUser);

        // act
        var result = await _controller.CreateUser(receivedDto);

        // assert
        Assert.IsType<CreatedAtRouteResult>(result);
        _mockService.Verify(s => s.CreateUser(It.IsAny<UserCreateRecord>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserCallsDeleteUser_WhenNotSelf()
    {
        // Arrange
        const int id = 1;

        // mock the logged-in user with a DIFFERENT id
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, "999") // not equal to id
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.DeleteUser(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteUser(id), Times.Once);
    }

    [Fact]
    public async Task DeleteUserReturns500OnNonExistingUser()
    {
        // setup
        const int id = 1;
        _mockService.Setup(s => s.DeleteUser(id)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.DeleteUser(id);

        // assert
        var res = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, res.StatusCode);
    }

    [Fact]
    public async Task DeleteUserReturns500OnFail()
    {
        // setup
        const int id = 1;
        _mockService.Setup(s => s.DeleteUser(id)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.DeleteUser(id);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task UserIsAdminReturnsTrueForAdmin()
    {
        // setup
        List<Claim> claims = [new Claim(ClaimTypes.Role, "Admin")];
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = user
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // act
        var result = await _controller.UserIsAdmin();

        // assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(actionResult.Value, true);
    }

    [Fact]
    public async Task UserIsAdminReturnsFalseForUser()
    {
        // setup
        List<Claim> claims = [new Claim(ClaimTypes.Role, "User")];
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = user
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // act
        var result = await _controller.UserIsAdmin();

        // assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(actionResult.Value, false);
    }

    [Fact]
    public async Task GetUser_ReturnsOkWithUserDto_WhenUserExists()
    {
        // Arrange
        var user = DummyUser();
        const string role = "User";
        SetAuthenticatedUser(user.Id);
        _mockService.Setup(s => s.GetUser(user.Id)).ReturnsAsync(user);
        _mockService.Setup(s => s.GetUserRole(user)).ReturnsAsync(role);

        // Act
        var result = await _controller.GetUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsAssignableFrom<UserDto>(okResult.Value);
        Assert.Equal(user.Id, dto.Id);
    }

    [Fact]
    public async Task GetUser_Returns500_WhenServiceThrows()
    {
        // Arrange
        const int userId = 1;
        SetAuthenticatedUser(userId);
        _mockService.Setup(s => s.GetUser(userId)).ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _controller.GetUser();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetUser_CallsServiceWithAuthenticatedUserId()
    {
        // Arrange
        var user = DummyUser();
        SetAuthenticatedUser(user.Id);
        _mockService.Setup(s => s.GetUser(user.Id)).ReturnsAsync(user);
        _mockService.Setup(s => s.GetUserRole(user)).ReturnsAsync("User");

        // Act
        await _controller.GetUser();

        // Assert
        _mockService.Verify(s => s.GetUser(user.Id), Times.Once);
    }

    [Fact]
    public async Task GetUser_NeverCallsServiceForOtherUserId()
    {
        // Arrange — authenticated as user 2, should never fetch user 99
        SetAuthenticatedUser(2);
        _mockService.Setup(s => s.GetUser(2)).ReturnsAsync(new User { Id = 2 });
        _mockService.Setup(s => s.GetUserRole(It.IsAny<User>())).ReturnsAsync("User");

        // Act
        await _controller.GetUser();

        // Assert
        _mockService.Verify(s => s.GetUser(99), Times.Never);
    }

    [Fact]
    public async Task GetUserByName_ReturnsOkWithUserDto_WhenUserExists()
    {
        // Arrange
        var user = DummyUser();
        const string username = "JohnPork";
        const string role = "User";
        _mockService.Setup(s => s.GetUserByName(username)).ReturnsAsync(user);
        _mockService.Setup(s => s.GetUserRole(user)).ReturnsAsync(role);

        // Act
        var result = await _controller.GetUserByName(username);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsAssignableFrom<PublicUserDto>(okResult.Value);
        Assert.Equal(user.Id, dto.Id);
    }

    [Fact]
    public async Task GetUserByName_Returns500_WhenUserNotFound()
    {
        // Arrange
        const string username = "ghost";
        _mockService.Setup(s => s.GetUserByName(username)).ThrowsAsync(new Exception("Not found"));

        // Act
        var result = await _controller.GetUserByName(username);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetUserByName_Returns500_WhenGetUserRoleThrows()
    {
        // Arrange
        var user = DummyUser();
        const string username = "JohnPork";
        _mockService.Setup(s => s.GetUserByName(username)).ReturnsAsync(user);
        _mockService.Setup(s => s.GetUserRole(user)).ThrowsAsync(new Exception("Role lookup failed"));

        // Act
        var result = await _controller.GetUserByName(username);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetUserByName_CallsGetUserByNameWithCorrectUsername()
    {
        // Arrange
        var user = DummyUser();
        const string username = "JohnPork";
        _mockService.Setup(s => s.GetUserByName(username)).ReturnsAsync(user);
        _mockService.Setup(s => s.GetUserRole(user)).ReturnsAsync("User");

        // Act
        await _controller.GetUserByName(username);

        // Assert
        _mockService.Verify(s => s.GetUserByName(username), Times.Once);
    }

    [Fact]
    public async Task GetUserByName_CallsGetUserRoleAfterFetchingUser()
    {
        // Arrange
        var user = DummyUser();
        const string username = "JohnPork";
        _mockService.Setup(s => s.GetUserByName(username)).ReturnsAsync(user);
        _mockService.Setup(s => s.GetUserRole(user)).ReturnsAsync("Admin");

        // Act
        await _controller.GetUserByName(username);

        // Assert
        _mockService.Verify(s => s.GetUserRole(user), Times.Once);
    }

    [Fact]
    public async Task GetUserByName_DtoContainsCorrectRole()
    {
        // Arrange
        var user = DummyUser();
        const string username = "JohnPork";
        const string role = "Admin";
        _mockService.Setup(s => s.GetUserByName(username)).ReturnsAsync(user);
        _mockService.Setup(s => s.GetUserRole(user)).ReturnsAsync(role);

        // Act
        var result = await _controller.GetUserByName(username);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsAssignableFrom<PublicUserDto>(okResult.Value);
        Assert.Equal(role, dto.Role);
    }

    public async Task GetUserStreak_ReturnsStreak_WhenFound()
    {
        // setup
        var streak = new UserStreak { UserId = 1, CurrentStreak = 5, HighestStreak = 10 };
        _mockService.Setup(s => s.GetUserStreak(1)).ReturnsAsync(streak);

        // act
        var result = await _controller.GetUserStreak(1);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsAssignableFrom<UserStreakDto>(okResult.Value);
        Assert.Equal(1, dto.UserId);
        Assert.Equal(5, dto.CurrentStreak);
        Assert.Equal(10, dto.HighestStreak);
    }

    [Fact]
    public async Task GetUserStreak_Returns404_WhenNotFound()
    {
        // setup
        _mockService.Setup(s => s.GetUserStreak(999)).ThrowsAsync(new KeyNotFoundException());

        // act
        var result = await _controller.GetUserStreak(999);

        // assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User/Streak not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetUserStreak_Returns500_OnException()
    {
        // setup
        _mockService.Setup(s => s.GetUserStreak(1)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.GetUserStreak(1);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
