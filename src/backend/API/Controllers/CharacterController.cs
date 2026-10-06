/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers.DTOs;
using Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Tools.Auth;

namespace API.Controllers;

[ApiController]
[Route("api/characters")]
public class CharacterController(ICharacterService characterService, ILogger<CharacterController> logger) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CharacterDTO>>> GetCharacters()
    {
        try
        {
            var characters = await characterService.GetCharacters();

            var characterDTOs = characters.Select(CharacterDTO.CreateCharacterDTO).ToArray();

            return Ok(characterDTOs);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return Problem("A problem occurred when retrieving the characters");
        }
    }

    [Authorize]
    [HttpGet("user")]
    public async Task<ActionResult<IEnumerable<UserCharacterDTO>>> GetUserCharacters()
    {
        try
        {
            int userId = this.GetAuthenticatedUserId();
            var userCharacters = await characterService.GetUserCharacters(userId);
            var characterDTOs = userCharacters.Select(UserCharacterDTO.CreateUserCharacterDTO).ToArray();

            return Ok(characterDTOs);
        }
        catch (KeyNotFoundException keyError)
        {
            logger.LogError(keyError.Message);
            return NotFound("The requested user does not exist or has no character");
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return Problem("A problem occurred when retrieving characters for this user");
        }
    }

    [Authorize]
    [HttpGet("{userId}/selected")]
    public async Task<ActionResult<CharacterDTO>> GetSelectedUserCharacter(int userId)
    {
        try
        {
            UserCharacter character = await characterService.GetSelectedUserCharacter(userId);

            var userCharacterDTO = UserCharacterDTO.CreateUserCharacterDTO(character);

            return Ok(userCharacterDTO);
        }
        catch (KeyNotFoundException keyError)
        {
            logger.LogError(keyError.Message);
            return NotFound("The requested user has no character selected");
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return Problem("A problem occurred when retrieving the selected character");
        }
    }

    [Authorize]
    [HttpPut("user/selected")]
    public async Task<IActionResult> SelectUserCharacter(UserCharacterDTO userCharacterDTO)
    {
        try
        {
            int userId = this.GetAuthenticatedUserId();
            await characterService.SetSelectedCharacter(userId, userCharacterDTO.Id, userCharacterDTO.Palette);

            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex.Message);
            return NotFound("Could not find user or character when selecting character");
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return Problem("A problem occurred when updating the selected character");
        }
    }
}