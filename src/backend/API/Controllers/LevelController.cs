/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Security.Claims;
using API.Controllers.DTOs;
using Models;
using API.Services;
using API.Tools.Auth;
using API.Tools.QTIConverting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/level")]
public class LevelController(ILevelService levelService, ILogger<LevelController> logger) : ControllerBase
{

    /// <summary>
    /// Creates a random level with the given parameters. Does not reuse questions the user already got.
    /// </summary>
    /// <param name="size">amount of items the level contains</param>
    /// <param name="topicId">Topic ID of the topic used in the level</param>
    /// <param name="language">Language of the items used in the level. Default value is Dutch</param>
    /// <returns>An assessment xml file containing the given files</returns>
    [Authorize]
    [HttpPost("randomlevel")]
    public async Task<ActionResult<LevelResult>> GetRandomLevel([FromBody] int? topicId = null, Item.Language? language = Item.Language.Dutch)
    {
        if (topicId == null) return Problem("Domain ID not specified");
        try
        {
            int userId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id) ? id : 1;
            var (assessmentXml, items, level) = await levelService.GetRandomLevel(userId, (int)topicId, language.Value);

            var dto = new LevelResponseDto { AssessmentXml = assessmentXml, LevelId = level.Id, ItemIds = [.. items.Select(i => i.Id)] };

            return Ok(dto);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);

            if (e.Message.Contains("Not enough questions"))
                return Problem("Not enough questions available for this topic", null, 409);

            return Problem("An error occured while creating a new level");
        }
    }

    /// <summary>
    /// Handles submission of a user's answer for a specific <see cref="Item"/> within a given <see cref="LevelResult"/>.
    /// it creates a new <see cref="UserAnswer"/> or updates an existing one.
    /// </summary>
    /// <param name="dto">
    /// The <see cref="UserAnswerRequestDto"/> containing the identifiers for the <see cref="Item"/> and <see cref="LevelResult"/>,
    /// along with the user's answer text, answer identifier, correctness flag, and completion status.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the outcome of the operation, including success or not found if the <see cref="Item"/> does not exist.
    /// </returns>
    [Authorize]
    [HttpPost("submitAnswer")]
    public async Task<IActionResult> SubmitAnswer([FromBody] UserAnswerRequestDto dto)
    {
        try
        {
            int requestingUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            int? levelOwnerId = await levelService.GetLevelOwnerId(dto.LevelId);
            if (levelOwnerId == null) return NotFound("Level not found");
            if (levelOwnerId != requestingUserId && !User.IsInRole(Roles.Admin))
                return Forbid();

            var userAnswerRequest = UserAnswerRequestDto.FromDto(dto);
            await levelService.UpdateUserAnswer(userAnswerRequest);
            return Ok();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while submitting user answer");
        }
    }

    /// <summary>
    /// Submit a full level with a number of levelAnswers.
    /// </summary>
    /// <param name="dto">A Dto containing a levelResultId and a number of userAnswerDto's</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the operation</returns>
    [Authorize]
    [HttpPost("submitLevel")]
    public async Task<IActionResult> SubmitLevel([FromBody] LevelAnswerRequestDto dto)
    {
        int defaultCurrencyId = 1;
        try
        {
            int requestingUserId = this.GetAuthenticatedUserId();
            int? levelOwnerId = await levelService.GetLevelOwnerId(dto.LevelResultId);
            if (levelOwnerId == null) return NotFound("Level not found");
            if (levelOwnerId != requestingUserId && !User.IsInRole(Roles.Admin))
                return Forbid();

            var levelAnswerRequest = LevelAnswerRequestDto.FromDto(dto);
            var level = await levelService.SubmitLevel(levelAnswerRequest, logger);

            await levelService.GrantLevelRewards(level, requestingUserId, defaultCurrencyId);

            return Ok(level);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while submitting level");
        }
    }
}
