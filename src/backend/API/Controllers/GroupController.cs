/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers.DTOs;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/groups")]
public class GroupsController(IGroupService service) : ControllerBase
{
    /// <summary>
    /// Creates a new group
    /// </summary>
    [HttpPost("add")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        try
        {
            var group = await service.CreateGroup(dto.Name);
            return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return Problem("An error occurred while creating the group.");
        }
    }

    /// <summary>
    /// Retrieves a single group by its ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroup(int id)
    {
        try
        {
            var group = await service.GetGroup(id);
            return Ok(group);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return Problem("An error occurred while retrieving the group.");
        }
    }

    /// <summary>
    /// Adds a user as a member of the specified group.
    /// </summary>
    [HttpPost("{groupId}/members/{userId}")]
    public async Task<IActionResult> AddMember(int groupId, int userId)
    {
        try
        {
            var member = await service.AddMember(groupId, userId);
            return Ok(member);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return Problem("An error occurred while adding the member to the group.");
        }
    }

    /// <summary>
    /// Removes a user from the specified group.
    /// </summary>
    [HttpDelete("{groupId}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(int groupId, int userId)
    {
        try
        {
            await service.RemoveMember(groupId, userId);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return Problem("An error occurred while removing the member from the group.");
        }
    }

    /// <summary>
    /// Returns a paginated list of groups.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetGroups(int offset = 0, int range = 50)
    {
        try
        {
            var groups = await service.GetGroups(offset, range);
            return Ok(groups);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return Problem("An error occurred while retrieving groups.");
        }
    }

    /// <summary>
    /// Updates the name of an existing group.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateGroupDto dto)
    {
        try
        {
            var group = await service.UpdateGroup(id, dto.Name);
            return Ok(group);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return Problem("An error occurred while updating the group.");
        }
    }

    /// <summary>
    /// Deletes a group by its ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        try
        {
            await service.DeleteGroup(id);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return Problem("An error occurred while deleting the group.");
        }
    }

    /// <summary>
    /// Calculates and returns the group's average proficiency, optionally filtered by topic.
    /// </summary>
    [HttpGet("{groupId}/proficiency")]
    public async Task<IActionResult> GetGroupProficiency(
        int groupId,
        [FromQuery] int? topicId,
        [FromQuery] List<int>? topicIds)
    {
        try
        {
            var (avg, filter) = await service.GetGroupProficiency(groupId, topicId, topicIds);

            return Ok(new { GroupId = groupId, TopicFilter = filter, AverageProficiency = avg });
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return Problem("An error occurred while calculating group proficiency.");
        }
    }

    /// <summary>
    /// User gets score
    /// this is older code and more of a proof of concept
    /// </summary>
    [HttpGet("getScore")]
    public async Task<ActionResult<ICollection<double>>> GetScore(int groupId, int totalWeeks)
    {
        try
        {
            var score = await service.GetScore(groupId, totalWeeks);
            return Ok(score);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while getting the group score");
        }
    }
}
