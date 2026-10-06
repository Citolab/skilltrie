/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers.DTOs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API.Controllers;

/// <summary>
/// A controller class containing endpoints for admins
/// to do crud operations on badges in a configurational manner
/// </summary>
/// <author>Armand Ayar</author>
[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/badges/")]
public class BadgeAdminController(IBadgeAdminService badgeAdminService) : ControllerBase
{
    /// <summary>
    /// See <see cref="BadgeAdminService.GetParameterizedBadgeTypes"/>.
    /// It will reduce the values to simply a name of the field and the type of the field. 
    /// </summary>
    [HttpGet("parameters")]
    public ActionResult<Dictionary<string, List<ParameterDTO>>> GetParameterizedBadgeTypes()
    {
        Dictionary<string, string> typeMapper = new Dictionary<string, string>
        {
            { "Topic", "TopicId" }
        };
        return badgeAdminService.GetParameterizedBadgeTypes().ToDictionary(
            e => e.Key, 
            e => 
                e.Value.Select(i => new ParameterDTO(
                    i.field.Name, 
                    typeMapper.GetValueOrDefault(i.attribute.EntryColumn, i.attribute.EntryColumn)
                )).ToList()
        );
    }
    
    /// <summary>
    /// An endpoint for admins to add a parameterized badge to the database
    /// </summary>
    /// <returns>An empty action result</returns>
    [HttpPost("parameterized-badge")]
    public async Task<ActionResult> AddParameterizedBadge(AddParameterizedBadgeDTO dto)
    {
        await badgeAdminService.AddParameterizedBadge(dto);
        return Empty;
    }
    
    /// <summary>
    /// A simple endpoint to receive all the badges in the application with their corresponding information.
    /// </summary>
    /// <returns>A list of <see cref="BadgeInfoDTO"/> containing the information of each badge</returns>
    [HttpGet]
    public async Task<ActionResult<List<BadgeInfoDTO>>> GetAllBadges()
    {
        List<Badge> badges = await badgeAdminService.GetAllBadges();
        return badges.Select(BadgeInfoDTO.FromBadge).ToList();
    }

    /// <summary>
    /// See <see cref="BadgeAdminService.UpdateBadgeState"/>.
    /// </summary>
    [HttpPut("state")]
    public async Task<ActionResult> UpdateBadgeState(UpdateBadgeStateDTO dto)
    {
        await badgeAdminService.UpdateBadgeState(dto);
        return Empty;
    }
    
    /// <summary>
    /// An endpoint for publishing a badge which is currently staging.
    /// </summary>
    /// <param name="identifier">The identifier of the badge to publish</param>
    [HttpPut("publish/{identifier}")]
    public async Task<ActionResult> PublishBadge(string identifier)
    {
        await badgeAdminService.PublishBadge(identifier);
        return Empty;
    }
    
    /// <summary>
    /// An endpoint for removing a badge which is staging.
    /// </summary>
    /// <param name="identifier">The identifier of the badge to remove</param>
    [HttpDelete("{identifier}")]
    public async Task<ActionResult> RemoveBadge(string identifier)
    {
        await badgeAdminService.RemoveBadge(identifier);
        return Empty;
    }
}