/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers;
using API.Controllers.DTOs;
using Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace APITests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    private void SetUser(string email)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email)
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
    }

    [Fact]
    public async Task RegisterReturnsOkOnSuccess()
    {
        // setup
        var dto = new UserCreateDto
        {
            Email = "test@test.com",
            DisplayName = "testuser",
            FirstName = "Test",
            LastName = "User",
            Password = "Password123!"
        };

        var rec = new UserCreateRecord
        {
            Email = "test@test.com",
            DisplayName = "testuser",
            FirstName = "Test",
            LastName = "User",
            Password = "Password123!"
        };

        _mockAuthService
            .Setup(s => s.RegisterUser(It.IsAny<UserCreateRecord>()))
            .ReturnsAsync(new Models.User { Id = 1, Email = "test@test.com" });

        // act
        var result = await _controller.Register(dto);

        // assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task LoginReturnsOkOnSuccess()
    {
        // setup
        var dto = new LoginRequestDto { Email = "test@test.com", Password = "Password123!" };
        var fakeUser = new User { Email = "test@test.com" };
        _mockAuthService.Setup(s => s.AuthenticateUserAsync(dto.Email, dto.Password)).ReturnsAsync(fakeUser);
        _mockAuthService.Setup(s => s.SignInAsync(fakeUser, It.IsAny<bool>())).Returns(Task.CompletedTask);

        // act
        var result = await _controller.Login(dto, useCookies: true);

        // assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task LoginReturnsProblemOnException()
    {
        // setup
        var dto = new LoginRequestDto { Email = "fail@test.com", Password = "wrong" };
        _mockAuthService.Setup(s => s.AuthenticateUserAsync(dto.Email, dto.Password)).ThrowsAsync(new Exception("Auth error"));

        // act
        var result = await _controller.Login(dto);

        // assert
        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task LogoutReturnsOk()
    {
        // setup
        _mockAuthService.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

        // act
        var result = await _controller.Logout();

        // assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task PingAuthReturnsEmail()
    {
        // setup
        SetUser("testuser@test.com");

        // act
        var result = await _controller.PingAuth();

        // assert
        var ok = Assert.IsType<OkObjectResult>(result);
        object value = ok.Value!;
        var emailProperty = value.GetType().GetProperty("Email");
        Assert.NotNull(emailProperty);
        Assert.Equal("testuser@test.com", emailProperty.GetValue(value));
    }


    // additional AuthService edge case tests

    [Fact]
    public async Task LoginReturnsProblemWhenUserDoesNotExist()
    {
        // setup
        var dto = new LoginRequestDto { Email = "nouser@test.com", Password = "pass" };
        _mockAuthService.Setup(s => s.AuthenticateUserAsync(dto.Email, dto.Password)).ThrowsAsync(new Exception("User does not exist."));

        // act
        var result = await _controller.Login(dto);

        // assert
        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task LoginReturnsProblemWhenAccountLocked()
    {
        // setup
        var dto = new LoginRequestDto { Email = "locked@test.com", Password = "pass" };
        _mockAuthService.Setup(s => s.AuthenticateUserAsync(dto.Email, dto.Password)).ThrowsAsync(new Exception("Account locked."));

        // act
        var result = await _controller.Login(dto);

        // assert
        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task LoginReturnsProblemWhenPasswordInvalid()
    {
        // setup
        var dto = new LoginRequestDto { Email = "test@test.com", Password = "wrongpass" };
        _mockAuthService.Setup(s => s.AuthenticateUserAsync(dto.Email, dto.Password)).ThrowsAsync(new Exception("Invalid credentials."));

        // act
        var result = await _controller.Login(dto);

        // assert
        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }
}
