/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers.DTOs;
using API.Services;
using API.Tools.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/badges")]
public class BadgeController(IBadgeService badgeService) : ControllerBase
{
    /// <summary>
    /// Returns the badges and their progress of the user with the given user id.
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<ActionResult<List<UserBadgeDTO>>> UserBadges(int userId)
    {
        return await badgeService.GetUserBadges(userId);
    }
    
    /// <summary>
    /// Returns the badges and their progress of the currently logged-in user.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<UserBadgeDTO>>> CurrentUserBadges()
    {
        int userId = this.GetAuthenticatedUserId();
        return await badgeService.GetUserBadges(userId);
    }
}