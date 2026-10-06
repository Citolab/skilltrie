/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using API.Controllers.DTOs;
using Models;
using API.Services;
using API.Tools.Auth;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Controllers;

/// <summary>
/// Topic Controller: defines endpoints for interacting with topics in the database.
/// </summary>
/// <param name="topicService">The class that will execute the interactions with the database for the <see cref="AppDbContext.Scopes"/>, <see cref="AppDbContext.ScopeMemberships"/> and <see cref="AppDbContext.UserScopeProgress"/> tables.</param>
[ApiController]
[Route("api/topics")]
public class TopicController(ITopicService topicService) : ControllerBase
{
    /// <summary>
    /// Returns all topics paired by id and full path.
    /// </summary>
    /// <returns>
    /// A <see cref="ActionResult"/> containing either an empty collection or a collection of <see cref="ScopeDTO"/> items.
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<ICollection<ScopeDTO>>> GetAllTopics()
    {
        try
        {
            var topics = await topicService.GetAllTopics();
            return Ok(topics);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while retrieving topics");
        }
    }

    /// <summary>
    /// An endpoint to retrieve all the topic dependencies
    /// </summary>
    /// <returns>An edge list of topics containing the from and to (topic) of the edge</returns>
    [HttpGet("dependencies")]
    public async Task<ActionResult<ICollection<ScopeDependencyDTO>>> GetTopicDependencies()
    {
        return Ok(await topicService.GetTopicDependencies());
    }

    /// <summary>
    /// An endpoint to receive user-specific information for each topic, like the proficiency and whether it is mastered
    /// </summary>
    /// <returns>A list of <see cref="UserTopicInfoDTO"/> which contains the user-specific information for a topic</returns>
    [Authorize]
    [HttpGet("user-status")]
    public async Task<ActionResult<ICollection<UserTopicInfoDTO>>> GetUserTopicInfo()
    {
        var currentUserId = this.GetAuthenticatedUserId();
        var userTopicInfos = await topicService.GetUserTopicInfo(currentUserId);
        return Ok(userTopicInfos);
    }

    /// <summary>
    /// Returns all topics a specific user has access to given their proficiencies and the topic dependencies
    /// </summary>
    /// <returns>
    /// An <see cref="OkResult"/> containing a collection of <see cref="ScopeDTO"/> instances if succesful, otherwise a <see cref="NotFoundResult"/>.
    /// </returns>
    [Authorize]
    [HttpGet("usertopics")]
    public async Task<ActionResult<ICollection<ScopeDTO>>> GetUserTopics()
    {
        var userId = this.GetAuthenticatedUserId();
        var unlocked = await topicService.GetUnlockedTopics(userId);
        return Ok(unlocked);
    }

    /// <summary>
    /// Update a <see cref="UserScopeProgress"/> proficiency given a topic and a user.
    /// </summary>
    /// <param name="topicId">The Id of the topic the user's proficiency is to be updated for.</param>
    /// <param name="userId">Optional userId, if not given it will be taken from the claim.</param>
    /// <returns>
    /// A <see cref="NoContentResult"/> on succesful update or a <see cref="NotFoundResult"/> or <see cref="ProblemHttpResult"/> on failure.
    /// </returns>
    [Authorize]
    [HttpPatch("updateproficiency/{topicId}")]
    public async Task<IActionResult> UpdateUserProficiency(int topicId, int? userId = null)
    {
        userId ??= this.GetAuthenticatedUserId();
        await topicService.UpdateUserProficiency(topicId, (int)userId);
        return NoContent();
    }

    [Authorize]
    [HttpGet("next-topic")]
    public async Task<ActionResult<ScopeDTO?>> GetNextSuggestedTopic()
    {
        var userid = this.GetAuthenticatedUserId();
        var topic = await topicService.GetNextSuggestedTopic(userid);
        return Ok(topic);
    }
}
