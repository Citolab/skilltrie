/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Services;
using API.Tools.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/tour")]
[Authorize]
public class TourController(ITourService tourService) : ControllerBase
{
    /// <summary>
    /// Returns whether the currently logged-in user has seen the given tour.
    /// </summary>
    [HttpGet("{tourKey}")]
    public ActionResult<bool> GetTourSeen(string tourKey)
    {
        int userId = this.GetAuthenticatedUserId();
        return tourService.GetTourSeen(userId, tourKey);
    }

    /// <summary>
    /// Marks the given tour as seen for the currently logged-in user by adding a userId and tourkey pair into the database. 
    /// </summary>
    [HttpPost("{tourKey}")]
    public ActionResult MarkTourSeen(string tourKey)
    {
        int userId = this.GetAuthenticatedUserId();
        tourService.MarkTourSeen(userId, tourKey);
        return Ok();
    }
}