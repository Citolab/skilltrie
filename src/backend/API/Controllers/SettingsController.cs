/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers.DTOs;
using Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller for creating, updating and querying <see cref="Setting"/> related data.
/// </summary>
/// <param name="settingsService">The <see cref="SettingsService"/></param>
[ApiController]
[Route("api/settings")]
public class SettingsController(SettingsService settingsService) : ControllerBase
{
    /// <summary>
    /// Get the current active <see cref="Setting"/>
    /// for the application.
    /// </summary>
    [HttpGet("active-setting")]
    public ActionResult<SettingDto> GetActiveSetting()
    {
        try
        {
            return SettingDto.CreateDto(settingsService.ActiveSetting);
        }
        catch
        {
            return Problem("There are no settings available");
        }
    }

    /// <summary>
    /// Activate a <see cref="Setting"/>,
    /// given its Id.
    /// </summary>
    /// <param name="id">The Id of the setting that is to be activated</param>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("activate-setting")]
    public async Task<IActionResult> ActivateSetting(int id)
    {
        try
        {

            var setting = await settingsService.GetSetting(id);
            if (setting == null) return NotFound("Setting was not found");

            settingsService.ActiveSetting = setting;

            return NoContent();
        }
        catch
        {
            return Problem("something went wrong activating this setting");
        }
    }

    /// <summary>
    /// Get multiple <see cref="Setting"/>s, paginated.
    /// </summary>
    /// <param name="offset">The offset</param>
    /// <param name="range">The range window from the offset</param>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("get-settings")]
    public ActionResult<ICollection<SettingDto>> GetSettings(int offset, int range)
    {
        try
        {
            var settings = settingsService.GetSettings(offset, range);

            return Ok(settings.Select(s => SettingDto.CreateDto(s)));
        }
        catch
        {
            return Problem("Pagination error");
        }
    }

    /// <summary>
    /// Create a new <see cref="Setting"/> entity.
    /// </summary>
    /// <param name="settingDto"></param>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("create")]
    public async Task<IActionResult> CreateSetting([FromBody] SettingDto settingDto)
    {
        try
        {
            if (!settingsService.Validate(settingDto))
            {
                return BadRequest("Could not create setting: invalid setting object");
            }

            var newSetting = SettingDto.FromDto(settingDto);

            await settingsService.AddSetting(newSetting);

            return NoContent();
        }
        catch
        {
            return Problem("Something went wrong adding a new setting");
        }
    }

    /// <summary>
    /// Update an existing <see cref="Setting"/>.
    /// </summary>
    /// <param name="settingDto">A Setting DTO</param>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateSetting([FromBody] SettingDto settingDto)
    {
        try
        {
            if (!settingsService.Validate(settingDto))
            {
                return BadRequest("Could not create setting: invalid setting object");
            }

            await settingsService.UpdateFromSetting(SettingDto.FromDto(settingDto));

            return NoContent();
        }
        catch
        {
            return Problem("Something went wrong while updating this setting");
        }
    }
}
