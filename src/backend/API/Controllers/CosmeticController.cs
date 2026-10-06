/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using API.Controllers.DTOs;
using Models;
using API.Services;

namespace API.Controllers;

/// <summary>
/// Cosmetic Controller: defines endpoints for retrieving cosmetics from the database
/// and assiging, buying and selling cosmetics to users.
/// </summary>
/// <param name="cosmeticService">The class that will execute the cosmetic database interactions for the Cosmetic table</param>
/// <param name="userCosmeticService">The class that will execute the cosmetic database interactions for the UserCosmetic table</param>
[ApiController]
[Route("api/cosmetics")]
public class CosmeticController(ICosmeticService cosmeticService, IUserCosmeticService userCosmeticService) : ControllerBase
{
    /// <summary>
    /// Retrieve the cosmetics from the database.
    /// </summary>
    /// <param name="clothingType">Optional. If specified, filters cosmetics based on <see cref="ClothingType"/></param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing a collection of <see cref="CosmeticDto"/> instances if successful.
    /// </returns>
    [HttpGet("cosmetics")]
    public async Task<ActionResult<IEnumerable<CosmeticDto>>> GetCosmetics(string? clothingType = null)
    {
        try
        {
            IEnumerable<Cosmetic> cosms;
            if (clothingType != null)
            {
                ClothingType type = Enum.Parse<ClothingType>(clothingType);
                cosms = await cosmeticService.GetCosmetics(type);
            }
            else
            {
                cosms = await cosmeticService.GetCosmetics();
            }

            IEnumerable<CosmeticDto> cosDtos = cosms.Select(CosmeticDto.CreateCosmeticDto);
            return Ok(cosDtos);
        }

        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while retrieving cosmetics");
        }
    }

    /// <summary>
    /// Retrieve the cosmetics of a user
    /// </summary>
    /// <param name="equipped"> Optional. If specified, filters cosmetics that the user has or hasn't equipped.</param>
    /// <param name="clothingType">Optional. If specified, filters cosmetics based on <see cref="ClothingType"/></param>
    /// <param name="userId">Optional. If specified, retrieves the cosmetics of a <see cref="User"/> with that userId.
    /// This requires a user with role <see cref="Roles.Admin"/>.
    /// If not specified, returns cosmetics of user currently logged in.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing a collection of <see cref="UserCosmeticDto"/> instances if successful.
    /// </returns>
    [Authorize]
    [HttpGet("usercosmetics")]
    public async Task<ActionResult<UserCosmeticDto>> GetUserCosmetics(bool? equipped, string? clothingType, [FromQuery] int? userId = null)
    {
        try
        {
            if (!VerifyTargetUserId(userId, out int userTargetId, out ActionResult? error))
                return error!;

            IEnumerable<UserCosmetic> cosms;
            if (clothingType != null)
            {
                ClothingType type = Enum.Parse<ClothingType>(clothingType);
                cosms = await userCosmeticService.GetUserCosmetics(userTargetId, equipped, type);
            }
            else
            {
                cosms = await userCosmeticService.GetUserCosmetics(userTargetId, equipped);
            }
            IEnumerable<UserCosmeticDto> userCurDtos = cosms.Select(UserCosmeticDto.CreateUserCosmeticDto);
            return Ok(userCurDtos);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while retrieving user cosmetics");
        }

    }

    /// <summary>
    /// Makes a request to buy a cosmetic for a <see cref="User"/>
    /// </summary>
    /// <param name="ucd"> The <see cref="UserCosmeticDto"/> containing the data on the user and cosmetic.</param>
    /// <returns>
    /// A <see cref="NoContentResult"/> on succesfull purchase.
    /// </returns>
    [Authorize]
    [HttpPost("usercosmetics/buy")]
    public async Task<ActionResult> BuyUserCosmetic(UserCosmeticDto ucd)
    {
        try
        {
            if (!VerifyTargetUserId(ucd.UserId, out int userTargetId, out ActionResult? error))
                return error!;
            var userCurBought = await userCosmeticService.BuyUserCosmetic(userTargetId, ucd.CosmeticId);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while purchasing cosmetic");
        }
    }
    /// <summary>
    /// Makes a request to equip or unequip a cosmetic for a <see cref="User"/>
    /// </summary>
    /// <param name="ucd"> The <see cref="UserCosmeticDto"/> containing the data on the user and cosmetic.</param>
    /// <returns>
    /// A <see cref="NoContentResult"/> on succesfull equip.
    /// </returns>
    [Authorize]
    [HttpPut("usercosmetics/equip")]
    public async Task<ActionResult> EquipUserCosmetic(UserCosmeticDto ucd)
    {
        try
        {
            if (!VerifyTargetUserId(ucd.UserId, out int userTargetId, out ActionResult? error))
                return error!;

            await userCosmeticService.EquipUserCosmetic(userTargetId, ucd.CosmeticId, ucd.Equipped);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while equipping cosmetic");
        }
    }

    /// <summary>
    /// Makes a request to sell a cosmetic for a <see cref="User"/>
    /// </summary>
    /// <param name="ucd"> The <see cref="UserCosmeticDto"/> containing the data on the user and cosmetic.</param>
    /// A <see cref="NoContentResult"/> on succesfull sell.

    [Authorize]
    [HttpDelete("usercosmetics/sell")]
    public async Task<ActionResult> SellUserCosmetic(UserCosmeticDto ucd)
    {
        try
        {
            if (!VerifyTargetUserId(ucd.UserId, out int userTargetId, out ActionResult? error))
                return error!;

            await userCosmeticService.SellUserCosmetic(userTargetId, ucd.CosmeticId);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while selling user cosmetic");
        }
    }

    // Helper function to check authorisation
    // Checks if a user, requesting data from another user, is in admin role.
    private bool VerifyTargetUserId(int? targetId, out int userTargetId, out ActionResult? error)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (targetId != null && targetId != userId)
        {
            if (!User.IsInRole(Roles.Admin))
            {
                error = Unauthorized();
                userTargetId = default;
                return false;
            }

        }
        userTargetId = targetId ?? userId;
        error = default;
        return true;
    }

}
