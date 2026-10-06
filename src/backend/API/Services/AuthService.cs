/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Security.Claims;
using API.Handlers.GameEventHandlers;
using Microsoft.AspNetCore.Identity;
using Models;

namespace API.Services;

public interface IAuthService
{
    Task<User> RegisterUser(UserCreateRecord ucr);
    Task<User> AuthenticateUserAsync(string email, string password);
    Task SignInAsync(User user, bool isPersistent);
    Task SignOutAsync();
}
public class AuthService(
    AppDbContext context,
    IUserService userService,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IGameEventManager gameEventManager) : IAuthService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="ucr">The <see cref="UserCreateRecord"/> with User Data</param>
    public async Task<User> RegisterUser(UserCreateRecord ucr)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var user = await userService.CreateUser(ucr);
            await SignInAsync(user, isPersistent: false);
            await transaction.CommitAsync();
            await gameEventManager.TriggerGameEvent(new UserRegisteredData { UserId = user.Id, User = user });
            return user;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Authenicate a user
    /// </summary>
    /// <param name="email">User's Email</param>
    /// <param name="password">User's Password</param>
    /// <returns>Returns the user if all went well</returns>
    /// <exception cref="Exception">Something went wrong while authenticating the user</exception>
    public async Task<User> AuthenticateUserAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email)
            ?? throw new Exception("User does not exist.");

        if (await userManager.IsLockedOutAsync(user))
            throw new Exception("Account locked.");

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            throw new Exception("Invalid credentials.");
        }

        return user;
    }

    /// <summary>
    /// Sign in a given <see cref="User"/>
    /// </summary>
    /// <param name="user">The <see cref="User"/> to be logged in</param>
    /// <param name="isPersistent">If the "remember me" box was ticked</param>
    /// <exception cref="Exception">Something went wrong while Signing in the user</exception>
    public async Task SignInAsync(User user, bool isPersistent)
    {
        try
        {
            await userManager.ResetAccessFailedCountAsync(user);

            // Add claims to the user
            await userService.AddClaimAsync(user, ClaimTypes.NameIdentifier, user.Id.ToString());
            await userService.AddClaimAsync(user, ClaimTypes.Email, user.Email!);

            // Let Identity generate the cookie
            await signInManager.SignInAsync(user, isPersistent);
        }
        catch
        {
            throw new Exception("Failed to sign in user.");
        }
    }

    /// <summary>
    /// Sign out a user
    /// </summary>
    public async Task SignOutAsync()
    {
        await signInManager.SignOutAsync();
    }

    
}
