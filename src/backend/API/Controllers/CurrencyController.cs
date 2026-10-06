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
/// Currency Controller: defines endpoints for retrieving currencies from the database
/// and assigning currencies to users and updating the balance(s) of users.
/// </summary>
/// <param name="currencyService">The class that will execute the currency database interactions for the Currency table.</param>
/// <param name="userCurrencyService">The class that will execute the currency database interactions for the UserCurrency table.</param>

[ApiController]
[Route("api/currency")]
public class CurrencyController(
    ICurrencyService currencyService,
    IUserCurrencyService userCurrencyService
    ) : ControllerBase
{
    /// <summary>
    /// Retrieves the currencies from the database.
    /// </summary>
    /// <returns>
    /// <see cref="ActionResult"/> containing a collection of <see cref="CurrencyDto"/> instances if successful.
    /// </returns>
    [HttpGet("currencies")]
    public async Task<ActionResult<IEnumerable<CurrencyDto>>> GetCurrencies()
    {
        try
        {
            var curs = await currencyService.GetCurrencies();
            IEnumerable<CurrencyDto> curDtos = curs.Select(CurrencyDto.CreateCurrencyDto);
            return Ok(curDtos);
        }

        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while retrieving currencies");
        }
    }

    /// <summary>
    /// Retrieves a currency from the database by it's identifier.
    /// </summary>
    /// <returns>
    /// <see cref="ActionResult"/> containing a <see cref="CurrencyDto"/> instance if successful.
    /// </returns>
    [HttpGet("currency/{id}")]
    public async Task<ActionResult<CurrencyDto>> GetCurrency(int id)
    {
        try
        {
            var cur = await currencyService.GetCurrency(id);
            CurrencyDto curDto = CurrencyDto.CreateCurrencyDto(cur);
            return Ok(curDto);

        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while retrieving currency");
        }
    }

    /// <summary>
    /// Retrieves the currencies of a user
    /// </summary>
    /// <param name="userId"> Optional. If specified, retrieves the cosmetics of a <see cref="User"/> with that userId.
    /// This requires a user with role <see cref="Roles.Admin"/>.
    /// If not specified, returns cosmetics of user currently logged in.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing a collection of <see cref="UserCurrencyDto"/> instances if successful.
    /// </returns>
    [Authorize]
    [HttpGet("usercurrency")]
    public async Task<ActionResult<IEnumerable<UserCurrencyDto>>> GetUserCurrencies([FromQuery] int? userId = null)
    {
        try
        {
            if (!VerifyTargetUserId(userId, out int userTargetId, out ActionResult? error))
                return error!;

            var curs = await userCurrencyService.GetUserCurrencies(userTargetId);
            IEnumerable<UserCurrencyDto> curDtos = curs.Select(UserCurrencyDto.CreateUserCurrencyDto);
            return Ok(curDtos);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while retrieving user currencies");
        }
    }

    /// <summary>
    /// Makes a request to update a the currency balance of a user
    /// </summary>
    /// <param name="ucd"> The <see cref="UserCurrencyUpdateDto"/> containing the data on the currency and the update amount.</param>
    /// <param name="userId"> Optional. If specified, retrieves the cosmetics of a <see cref="User"/> with that userId.
    /// This requires a user with role <see cref="Roles.Admin"/>.
    /// If not specified, returns cosmetics of user currently logged in.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> containing a <see cref="UserCurrencyDto"/> instance if successful.
    /// </returns>
    [Authorize]
    [HttpPut("usercurrency/update")]
    public async Task<ActionResult<UserCurrencyDto>> UpdateUserCurrency(UserCurrencyUpdateDto ucd, [FromQuery] int? userId = null)
    {
        try
        {
            if (!VerifyTargetUserId(userId, out int userTargetId, out ActionResult? error))
                return error!;

            var updatedCur = await userCurrencyService.UpdateUserCurrency(userTargetId, ucd.CurrencyId, ucd.Amount);
            UserCurrencyDto dto = UserCurrencyDto.CreateUserCurrencyDto(updatedCur);
            return Ok(dto);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while updating user currency");
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


