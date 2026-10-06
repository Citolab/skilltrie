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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace APITests.Controllers;

public class ReportControllerTests
{
    private readonly Mock<IReportService> _mockService;

    public ReportControllerTests()
    {
        _mockService = new Mock<IReportService>();
    }

    private static ReportDto DummyReportDto()
    {
        return new ReportDto { ItemId = 1, ItemError = "QTextEmpty" };
    }

    [Fact(DisplayName = "CreatesReport functional on valid input")]
    public async Task CreateReportCallsCreateReport()
    {
        // setup
        var dto = DummyReportDto();
        ReportController controller = CreateController(5);

        // act
        var result = await controller.CreateReport(dto);

        // assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.CreateReport(1, ItemError.QTextEmpty, 5), Times.Once);
    }

    [Fact(DisplayName = "CreatesReport throws 500 error on invalid item error and does not call service")]
    public async Task ThrowError_OnInvalidItemError()
    {
        var service = new Mock<IReportService>();
        ReportController controller = CreateController(5);

        var dto = new ReportDto
        {
            ItemId = 1,
            ItemError = "InvalidError"
        };

        var result = await controller.CreateReport(dto);
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode);

        service.Verify(s => s.CreateReport(It.IsAny<int>(), It.IsAny<ItemError>(), It.IsAny<int>()), Times.Never);
    }

    private ReportController CreateController(int userId)
    {
        var controller = new ReportController(_mockService.Object);

        // Mock the ClaimsPrincipal
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Assign it to the controller
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        return controller;
    }
}
