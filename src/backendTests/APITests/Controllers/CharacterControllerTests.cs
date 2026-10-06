/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers.DTOs;
using Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using API.Controllers;
using Microsoft.Extensions.Logging;

namespace APITests.Controllers;


public class CharacterControllerTests
{
    private readonly Mock<ICharacterService> _mockCharacterService;
    private readonly CharacterController _controller;

    public CharacterControllerTests()
    {
        _mockCharacterService = new Mock<ICharacterService>();

        _controller = new CharacterController(_mockCharacterService.Object, Mock.Of<ILogger<CharacterController>>());
    }

    private static Character DummyCharacter(int id, string name)
        => new Character { Id = id, Name = name };

    private static UserCharacter DummyUserCharacter(int userId, int characterId, bool selected = false)
    => new UserCharacter
    {
        UserId = userId,
        CharacterId = characterId,
        Selected = selected,
        Palette = "classic",
        Character = DummyCharacter(characterId, "classic")
    };

    private void SetUser(int userId, bool isAdmin = false)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, Roles.Admin));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
    }

    [Fact]
    public async Task GetsSingleCharacterSuccessfully()
    {
        // setup
        var list = new List<Character> { DummyCharacter(1, "dog") };
        _mockCharacterService.Setup(s => s.GetCharacters()).ReturnsAsync(list);

        // act
        var result = await _controller.GetCharacters();

        // assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<CharacterDTO>>(ok.Value!);
        Assert.Single(dtos);
    }

    [Fact]
    public async Task GetsMultipleCharactersSuccessfully()
    {
        // setup
        var list = new List<Character> { DummyCharacter(1, "dog"), DummyCharacter(2, "dog"), DummyCharacter(3, "dog"), DummyCharacter(4, "dog") };
        _mockCharacterService.Setup(s => s.GetCharacters()).ReturnsAsync(list);

        // act
        var result = await _controller.GetCharacters();

        // assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<CharacterDTO>>(ok.Value!);
        Assert.Equal(4, dtos.Count());
    }

    [Fact]
    public async Task GetCharactersReturnsProblemOnException()
    {
        // setup
        _mockCharacterService.Setup(s => s.GetCharacters()).ThrowsAsync(new Exception());

        // act
        var result = await _controller.GetCharacters();

        // assert
        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task GetUserCharacters_ReturnsOk_WithCharacterDTOs()
    {
        // Arrange
        const int userId = 1;
        SetUser(userId);
        var userCharacters = new List<UserCharacter>
        {
            DummyUserCharacter(userId, 1),
            DummyUserCharacter(userId, 2)
        };

        _mockCharacterService
            .Setup(s => s.GetUserCharacters(userId))
            .ReturnsAsync(userCharacters);

        // Act
        var result = await _controller.GetUserCharacters();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<UserCharacterDTO>>(okResult.Value);
        Assert.Equal(2, dtos.Count());
    }

    [Fact]
    public async Task GetUserCharacters_ReturnsNotFound_WhenKeyNotFoundExceptionThrown()
    {
        // Arrange
        const int userId = 99;
        SetUser(userId);
        _mockCharacterService
            .Setup(s => s.GetUserCharacters(userId))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        // Act
        var result = await _controller.GetUserCharacters();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("The requested user does not exist or has no character", notFoundResult.Value);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetUserCharacters_ReturnsProblem_WhenUnexpectedExceptionThrown()
    {
        // Arrange
        const int userId = 1;
        SetUser(userId);
        _mockCharacterService
            .Setup(s => s.GetUserCharacters(userId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.GetUserCharacters();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("A problem occurred when retrieving characters for this user", problemDetails.Detail);
    }

    [Fact]
    public async Task GetUserCharacters_ReturnsEmptyList_WhenUserHasNoCharacters()
    {
        // Arrange
        const int userId = 1;
        SetUser(userId);
        _mockCharacterService
            .Setup(s => s.GetUserCharacters(userId))
            .ReturnsAsync(new List<UserCharacter>());

        // Act
        var result = await _controller.GetUserCharacters();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<UserCharacterDTO>>(okResult.Value);
        Assert.Empty(dtos);
    }

    [Fact]
    public async Task GetSelectedUserCharacter_ReturnsOk_WithSelectedCharacterDTO()
    {
        // Arrange
        const int userId = 1;
        var userCharacter = DummyUserCharacter(userId, 1, selected: true);

        _mockCharacterService
            .Setup(s => s.GetSelectedUserCharacter(userId))
            .ReturnsAsync(userCharacter);

        // Act
        var result = await _controller.GetSelectedUserCharacter(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<UserCharacterDTO>(okResult.Value);
    }

    [Fact]
    public async Task GetSelectedUserCharacter_ReturnsNotFound_WhenKeyNotFoundExceptionThrown()
    {
        // Arrange
        const int userId = 99;
        _mockCharacterService
            .Setup(s => s.GetSelectedUserCharacter(userId))
            .ThrowsAsync(new KeyNotFoundException("No selected character"));

        // Act
        var result = await _controller.GetSelectedUserCharacter(userId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("The requested user has no character selected", notFoundResult.Value);
    }

    [Fact]
    public async Task GetSelectedUserCharacter_ReturnsProblem_WhenUnexpectedExceptionThrown()
    {
        // Arrange
        const int userId = 1;
        _mockCharacterService
            .Setup(s => s.GetSelectedUserCharacter(userId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.GetSelectedUserCharacter(userId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("A problem occurred when retrieving the selected character", problemDetails.Detail);
    }

    [Fact]
    public async Task GetSelectedUserCharacter_ReturnsDTOWithCorrectUserId()
    {
        // Arrange
        const int userId = 42;
        var userCharacter = DummyUserCharacter(userId, 1, selected: true);

        _mockCharacterService
            .Setup(s => s.GetSelectedUserCharacter(userId))
            .ReturnsAsync(userCharacter);

        // Act
        var result = await _controller.GetSelectedUserCharacter(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<UserCharacterDTO>(okResult.Value);
        Assert.Equal(1, dto.Id);
    }

    [Fact]
    public async Task SelectUserCharacter_ReturnsOk_WhenSelectionSucceeds()
    {
        // Arrange
        const int userId = 1;
        UserCharacterDTO dto = new UserCharacterDTO()
        {
            Id = 1,
            Name = "character",
            Palette = "default",
            Selected = false
        };

        SetUser(userId);

        _mockCharacterService
            .Setup(s => s.SetSelectedCharacter(userId, dto.Id, dto.Palette))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SelectUserCharacter(dto);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task SelectUserCharacter_CallsService_WithAuthenticatedUserId()
    {
        // Arrange
        const int userId = 7;
        UserCharacterDTO dto = new UserCharacterDTO()
        {
            Id = 1,
            Name = "character",
            Palette = "fire",
            Selected = false
        };
        SetUser(userId);

        _mockCharacterService
            .Setup(s => s.SetSelectedCharacter(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.SelectUserCharacter(dto);

        // Assert
        _mockCharacterService.Verify(
            s => s.SetSelectedCharacter(userId, dto.Id, dto.Palette),
            Times.Once
        );
    }

    [Fact]
    public async Task SelectUserCharacter_ReturnsNotFound_WhenKeyNotFoundExceptionThrown()
    {
        // Arrange
        const int userId = 99;
        UserCharacterDTO dto = new UserCharacterDTO()
        {
            Id = 1,
            Name = "character",
            Palette = "fire",
            Selected = false
        };
        SetUser(userId);

        _mockCharacterService
            .Setup(s => s.SetSelectedCharacter(userId, It.IsAny<int>(), It.IsAny<string>()))
            .ThrowsAsync(new KeyNotFoundException("No selected character"));

        // Act
        var result = await _controller.SelectUserCharacter(dto);

        // Assert
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Could not find user or character when selecting character", notFoundResult.Value);
    }

    [Fact]
    public async Task SelectUserCharacter_ReturnsProblem_WhenUnexpectedExceptionThrown()
    {
        // Arrange
        const int userId = 1;
        UserCharacterDTO dto = new UserCharacterDTO()
        {
            Id = 1,
            Name = "character",
            Palette = "default",
            Selected = false
        };
        SetUser(userId);

        _mockCharacterService
            .Setup(s => s.SetSelectedCharacter(userId, dto.Id, dto.Palette))
            .ThrowsAsync(new Exception("DB failure"));

        // Act
        var result = await _controller.SelectUserCharacter(dto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("A problem occurred when updating the selected character", problemDetails.Detail);
    }

    [Fact]
    public async Task SelectUserCharacter_AsAdmin_ReturnsOk()
    {
        // Arrange
        const int userId = 2;
        UserCharacterDTO dto = new UserCharacterDTO()
        {
            Id = 5,
            Name = "character",
            Palette = "ice",
            Selected = false
        };
        SetUser(userId, isAdmin: true);

        _mockCharacterService
            .Setup(s => s.SetSelectedCharacter(userId, dto.Id, dto.Palette))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SelectUserCharacter(dto);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task SelectUserCharacter_CallsService_ExactlyOnce()
    {
        // Arrange
        const int userId = 3;
        UserCharacterDTO dto = new UserCharacterDTO()
        {
            Id = 8,
            Name = "character",
            Palette = "shadow",
            Selected = false
        };
        SetUser(userId);

        _mockCharacterService
            .Setup(s => s.SetSelectedCharacter(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.SelectUserCharacter(dto);

        // Assert
        _mockCharacterService.Verify(
            s => s.SetSelectedCharacter(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.Once
        );
    }

}
