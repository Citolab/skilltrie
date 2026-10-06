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
using Microsoft.Extensions.Logging;
using Moq;

namespace APITests.Controllers;

public class LevelControllerTests
{
    private readonly Mock<ILevelService> _mockService;
    private readonly LevelController _controller;

    public LevelControllerTests()
    {
        _mockService = new Mock<ILevelService>();
        _controller = new LevelController(_mockService.Object, null!);
    }

    private static LevelAnswerRequestDto DummyLevelAnswerRequestDto()
    {
        return new LevelAnswerRequestDto();
    }

    private static UserAnswerRequestDto DummyUserAnswerRequestDto()
    {
        return new UserAnswerRequestDto();
    }

    [Fact]
    public async Task GetRandomLevelReturns500OnFail()
    {
        // setup
        const int userId = 1;
        _mockService.Setup(s => s.GetRandomLevel(userId, 2, Item.Language.Dutch)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.GetRandomLevel(2);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task SubmitAnswerReturns500OnFail()
    {
        // setup
        var dto = DummyUserAnswerRequestDto();
        _mockService
            .Setup(s => s.UpdateUserAnswer(It.IsAny<UserAnswerRequest>()))
            .ThrowsAsync(new Exception());

        // act
        var result = await _controller.SubmitAnswer(dto);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task SubmitLevelReturns500OnFail()
    {
        // setup
        var dto = DummyLevelAnswerRequestDto();
        _mockService
            .Setup(s => s.SubmitLevel(It.IsAny<LevelAnswerRequest>(), It.IsAny<ILogger>()))
            .ThrowsAsync(new Exception());

        // act
        var result = await _controller.SubmitLevel(dto);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
