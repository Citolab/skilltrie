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
using Moq;

namespace APITests.Controllers;

public class ResearcherDashboardControllerTests
{
    private static MetricDTO CreateTestMetrics(int studentCount = 0, int activeUserCount = 0, int flaggedItemCount = 0)
    {
        return new MetricDTO
        {
            StudentCount = studentCount,
            ActiveUserCount = activeUserCount,
            TestsTakenPastWeek = [],
            FlaggedItemCount = flaggedItemCount
        };
    }

    [Fact(DisplayName = "getMetrics: Returns 200 with metrics on success")]
    public async Task getMetrics_Returns200WithMetrics()
    {
        var metrics = CreateTestMetrics(studentCount: 10, activeUserCount: 5, flaggedItemCount: 3);
        var service = new Mock<IResearcherDashboardService>();
        service.Setup(s => s.getMetrics()).ReturnsAsync(metrics);

        var controller = new ResearcherDashboardController(service.Object);
        var result = await controller.getMetrics();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(metrics, okResult.Value);
    }

    [Fact(DisplayName = "getMetrics: Returns 500 when service throws")]
    public async Task getMetrics_Returns500WhenServiceThrows()
    {
        var service = new Mock<IResearcherDashboardService>();
        service.Setup(s => s.getMetrics()).ThrowsAsync(new Exception("Database error"));

        var controller = new ResearcherDashboardController(service.Object);
        var result = await controller.getMetrics();

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("Database error", statusResult.Value);
    }

    [Fact(DisplayName = "getMetrics: Calls service exactly once")]
    public async Task getMetrics_CallsServiceOnce()
    {
        var service = new Mock<IResearcherDashboardService>();
        service.Setup(s => s.getMetrics()).ReturnsAsync(CreateTestMetrics());

        var controller = new ResearcherDashboardController(service.Object);
        await controller.getMetrics();

        service.Verify(s => s.getMetrics(), Times.Once);
    }
}