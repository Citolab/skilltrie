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
using API.Tools.Auth;

namespace API.Controllers;

/// <summary>
/// The controller that handles all the API calls regarding user information (getting, adding, updating and deleting).
/// </summary>
/// <param name="userService">The class that will execute the User and Identity database interactions and return the results to the controller.</param>
[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{

    /// <summary>
    /// Retrieve the user making the request from the database
    /// </summary>
    /// <returns>An <see cref="OkObjectResult"/> containing the UserDTO of the requested user. Returns a <see cref="ObjectResult"/> when the user cannot be found.</returns>
    [Authorize]
    [HttpGet("user")]
    public async Task<ActionResult<UserDto>> GetUser()
    {
        try
        {
            int id = this.GetAuthenticatedUserId();
            return await GetUser(id);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while getting user");
        }
    }

    /// <summary>
    /// Retrieve a user based on the username from the database
    /// </summary>
    /// <param name="username">The username of the user to retrieve from the database</param>
    /// <returns>An <see cref="OkObjectResult"/> containing the UserDTO of the requested user. Returns a <see cref="NotFoundObjectResult"/> when the user cannot be found.</returns>
    [Authorize]
    [HttpGet("username/{username}")]
    public async Task<ActionResult<PublicUserDto>> GetUserByName(string username)
    {
        try
        {
            User user = await userService.GetUserByName(username);
            string role = await userService.GetUserRole(user);
            PublicUserDto publicUserDto = PublicUserDto.CreatePublicUserDto(user, role);

            return Ok(publicUserDto);
        }
        catch (KeyNotFoundException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return NotFound("Could not find user with this username");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while getting user");
        }
    }

    /// <summary>
    /// Retrieve a user from the database.
    /// </summary>
    /// <param name="id">The id of the user you are attempting to retrieve.</param>
    /// <returns>An <see cref="OkObjectResult"/> containing the UserDTO of the requested user. Returns a <see cref="NotFoundObjectResult"/> when the user cannot be found.</returns>
    [Authorize]
    [HttpGet("user/{id}", Name = nameof(GetUser))]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            if (this.GetAuthenticatedUserId() != id && !User.IsInRole(Roles.Admin))
            {
                return Forbid();
            }

            User user = await userService.GetUser(id);
            string role = await userService.GetUserRole(user);
            UserDto userDto = UserDto.CreateUserDto(user, role);

            return Ok(userDto);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while getting user");
        }
    }

    /// <summary>
    /// Retrieve all users currently in the database.
    /// </summary>
    /// <returns>An <see cref="OkObjectResult"/> containing a list of UserDTOs of all users.</returns>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(int offset, int range)
    {
        try
        {
            range = Math.Clamp(range, 1, 50);
            offset = Math.Max(0, offset);

            var users = await userService.GetUsers(range, offset);

            ICollection<UserDto> userDtos = [];
            foreach (User user in users)
            {
                string role = await userService.GetUserRole(user);
                userDtos.Add(UserDto.CreateUserDto(user, role));
            }

            return Ok(userDtos);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while getting users");
        }
    }

    /// <summary>
    /// Checks if a user is an admin. 
    /// </summary>
    /// <returns>True when the user has the admin role, False otherwise.</returns>
    [Authorize]
    [HttpGet("user/isadmin")]
    public async Task<ActionResult<bool>> UserIsAdmin()
    {
        try
        {
            bool isadmin = User.IsInRole(Roles.Admin);
            return Ok(isadmin);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.Message);
            return Problem("An error occurred while checking user role");
        }
    }

    /// <summary>
    /// Create a user to add to the database.
    /// </summary>
    /// <param name="dto">The UserCreateDto used specifically for creating and updating users. Not to be confused with UserDto, since that DTO is used for already existing users (and therefore contains an id, which would be impossible to pass in this context).</param>
    /// <returns>A <see cref="CreatedAtRouteResult"/> containing the UserDTO of the created user. If this process fails, returns an <see cref="ObjectResult"/> with the status code 500.</returns>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("add")]
    public async Task<ActionResult> CreateUser(UserCreateDto dto)
    {
        UserCreateRecord ucr = UserCreateDto.CreateRecordFromDto(dto);
        User user = await userService.CreateUser(ucr);

        return CreatedAtRoute(routeName: nameof(GetUser), routeValues: new { id = user.Id }, value: null);
    }

    /// <summary>
    /// Update a user in the database with new/changed data.
    /// </summary>
    /// <param name="id">The id of the user you want to update.</param>
    /// <param name="dto">The UserCreateDto used specifically for creating and updating users. Because we allow updating passwords using this, we use UserCreateDto, as UserDto does not have a field that can pass a password.</param>
    /// <returns>A <see cref="NoContentResult"/> on succesful update. If the process fails, returns an <see cref="ObjectResult"/> with the status code 500.</returns>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("update/{id}")]
    public async Task<ActionResult> UpdateUser(int id, UserCreateDto dto)
    {
        UserCreateRecord ucr = UserCreateDto.CreateRecordFromDto(dto);
        var user = await userService.UpdateUser(id, ucr)
            ?? throw new Exception("Failed to update user.");

        return NoContent();
    }

    /// <summary>
    /// Delete a user from the database.
    /// </summary>
    /// <param name="id">The id of the user you want to delete.</param>
    /// <returns> A <see cref="NoContentResult"/> if deletion was successful, a <see cref="NotFoundObjectResult"/> if user was not found, and a <see cref="BadRequestObjectResult"/> if deletion failed.</returns>
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var currentUserId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var output) ? output : 1;

            if (currentUserId == id)
            {
                return BadRequest("You cannot delete your own account while logged in.");
            }

            await userService.DeleteUser(id);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error  occured while deleting user");
        }
    }

    /// <summary>
    /// Get the streak data of a specific user.
    /// </summary>
    /// <param name="userId">The id of the user you want to fetch the streak data of.</param>
    /// <returns>A <see cref="ActionResult{int}"/> containing the user's streak data if successful. Else returns a <see cref="NotFoundResult"/> or <see cref="ObjectResult"/> with status code 500.</returns>
    [Authorize]
    [HttpGet("{userId}/streak", Name = nameof(GetUserStreak))]
    public async Task<ActionResult<int>> GetUserStreak(int userId)
    {
        try
        {
            var streak = await userService.GetUserStreak(userId);
            UserStreakDto streakDto = UserStreakDto.CreateUserStreakDto(streak);
            return Ok(streakDto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("User/Streak not found");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occurred while retrieving user streak");
        }
    }
}