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
using Xunit;

namespace APITests.Controllers;

public class GroupsControllerTests
{
    private readonly Mock<IGroupService> service;
    private readonly GroupsController controller;

    public GroupsControllerTests()
    {
        service = new Mock<IGroupService>();
        controller = new GroupsController(service.Object);
    }

    [Fact]
    public async Task CreateGroup_ReturnsCreated_WhenSuccessful()
    {
        var dto = new CreateGroupDto("My Group");
        var group = new Group { Id = 42, Name = "My Group" };

        service.Setup(s => s.CreateGroup("My Group"))
            .ReturnsAsync(group);

        var result = await controller.CreateGroup(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(GroupsController.GetGroup), created.ActionName);
        Assert.Equal(42, created.RouteValues["id"]);
        Assert.Equal(group, created.Value);
    }

    [Fact]
    public async Task CreateGroup_ReturnsProblem_WhenExceptionThrown()
    {
        service.Setup(s => s.CreateGroup(It.IsAny<string>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await controller.CreateGroup(new CreateGroupDto("X"));

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task GetGroup_ReturnsOk_WhenGroupExists()
    {
        var group = new Group { Id = 1, Name = "Test" };

        service.Setup(s => s.GetGroup(1))
            .ReturnsAsync(group);

        var result = await controller.GetGroup(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(group, ok.Value);
    }

    [Fact]
    public async Task GetGroup_ReturnsProblem_WhenExceptionThrown()
    {
        service.Setup(s => s.GetGroup(It.IsAny<int>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await controller.GetGroup(1);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task AddMember_ReturnsOk_WhenSuccessful()
    {
        var member = new GroupMember { UserId = 5, GroupId = 10 };

        service.Setup(s => s.AddMember(10, 5))
            .ReturnsAsync(member);

        var result = await controller.AddMember(10, 5);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(member, ok.Value);
    }

    [Fact]
    public async Task AddMember_ReturnsProblem_WhenExceptionThrown()
    {
        service.Setup(s => s.AddMember(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await controller.AddMember(1, 1);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_ReturnsNoContent_WhenSuccessful()
    {
        service.Setup(s => s.RemoveMember(10, 5))
            .Returns(Task.CompletedTask);

        var result = await controller.RemoveMember(10, 5);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemoveMember_ReturnsProblem_WhenExceptionThrown()
    {
        service.Setup(s => s.RemoveMember(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await controller.RemoveMember(1, 1);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task GetGroups_ReturnsOk_WithList()
    {
        var groups = new List<Group>
        {
            new() { Id = 1, Name = "A" },
            new() { Id = 2, Name = "B" }
        };

        service.Setup(s => s.GetGroups(0, 50))
            .ReturnsAsync(groups);

        var result = await controller.GetGroups();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(groups, ok.Value);
    }

    [Fact]
    public async Task GetGroups_ReturnsProblem_WhenExceptionThrown()
    {
        service.Setup(s => s.GetGroups(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await controller.GetGroups();

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task UpdateGroup_ReturnsOk_WhenSuccessful()
    {
        var dto = new UpdateGroupDto("New");
        var group = new Group { Id = 1, Name = "New" };

        service.Setup(s => s.UpdateGroup(1, "New"))
            .ReturnsAsync(group);

        var result = await controller.UpdateGroup(1, dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(group, ok.Value);
    }

    [Fact]
    public async Task UpdateGroup_ReturnsProblem_WhenExceptionThrown()
    {
        service.Setup(s => s.UpdateGroup(It.IsAny<int>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await controller.UpdateGroup(1, new UpdateGroupDto("X"));

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task DeleteGroup_ReturnsNoContent_WhenSuccessful()
    {
        service.Setup(s => s.DeleteGroup(1))
            .Returns(Task.CompletedTask);

        var result = await controller.DeleteGroup(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteGroup_ReturnsProblem_WhenExceptionThrown()
    {
        service.Setup(s => s.DeleteGroup(It.IsAny<int>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await controller.DeleteGroup(1);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task GetGroupProficiency_ReturnsOk_WhenSuccessful()
    {
        service.Setup(s => s.GetGroupProficiency(1, null, null))
            .ReturnsAsync((87.5, "All"));

        var result = await controller.GetGroupProficiency(1, null, null);

        var ok = Assert.IsType<OkObjectResult>(result);

        var payload = ok.Value!;
        var type = payload.GetType();

        Assert.Equal(1, type.GetProperty("GroupId")!.GetValue(payload));
        Assert.Equal(87.5, type.GetProperty("AverageProficiency")!.GetValue(payload));
        Assert.Equal("All", type.GetProperty("TopicFilter")!.GetValue(payload));
    }

    [Fact]
    public async Task GetGroupProficiency_ReturnsProblem_WhenExceptionThrown()
    {
        service.Setup(s => s.GetGroupProficiency(It.IsAny<int>(), null, null))
            .ThrowsAsync(new Exception("boom"));

        var result = await controller.GetGroupProficiency(1, null, null);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }
}
