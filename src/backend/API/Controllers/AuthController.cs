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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

/// <summary>
/// Handles user authentication, including registration, login, logout, and checking authentication status.
/// Uses Identity for user and role management.
/// </summary>
/// <param name="authService">The class that will execute the User and Identity database interactions for authentication tasks.</param>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Register a new user in the system, assign them the default role and sign them in.
    /// </summary>
    /// <param name="dto">The DTO containing registration information, including email, displayname, name, and password.</param>
    /// <returns>A <see cref="CreatedAtActionResult"/> with the new user, or <see cref="BadRequestObjectResult"/>/500 on failure.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserCreateDto dto)
    {
        UserCreateRecord ucr = UserCreateDto.CreateRecordFromDto(dto) with { Role = Roles.User };
        var user = await authService.RegisterUser(ucr);
        if (user.Email == null)
        {
            return Problem("An Error occurred while registering user");
        }
        return Ok(new AuthResponseDto() { Id = user.Id, Email = user.Email });
    }

    /// <summary>
    /// Log in a user using their email and password.
    /// </summary>
    /// <param name="dto">The DTO containing login credentials.</param>
    /// <param name="useCookies">Whether there should be stored cookies or if they should only exist for the session.</param>
    /// <returns>An <see cref="OkResult"/> if successful, <see cref="UnauthorizedObjectResult"/> for invalid credentials, or 423 if the account is locked.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto, bool useCookies = false)
    {
        try
        {
            var user = await authService.AuthenticateUserAsync(dto.Email, dto.Password);
            await authService.SignInAsync(user, isPersistent: useCookies);
            if (user.Email == null)
            {
                return Problem("An Error occurred while registering user");
            }
            return Ok(new AuthResponseDto(){ Id = user.Id, Email = user.Email });
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An Error occured while logging in");
        }
    }

    /// <summary>
    /// Log out the currently authenticated user.
    /// </summary>
    /// <returns>An <see cref="OkResult"/> on successful logout.</returns>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await authService.SignOutAsync();
        return Ok();
    }

    /// <summary>
    /// Check whether the current user is authenticated and retrieve their email.
    /// </summary>
    /// <returns>An <see cref="OkObjectResult"/> with the authenticated user's email.</returns>
    [Authorize]
    [HttpGet("pingauth")]
    public async Task<IActionResult> PingAuth()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        return Ok(new { Email = email });
    }

}

