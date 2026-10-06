/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Security.Claims;
using API.Handlers;
using Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using API.Controllers.DTOs;
using API.Handlers.GameEventHandlers;

namespace API.Services;

public interface IUserService
{
    Task VerifyUserExists(int id);
    Task<User> GetUser(int id);
    Task<User> GetUserByName(string username);
    Task<ICollection<User>> GetUsers(int range, int offset);
    Task<string> GetUserRole(User user);
    Task<User> CreateUser(UserCreateRecord ucr);
    Task<User> UpdateUser(int id, UserCreateRecord ucr);
    Task DeleteUser(int id);
    Task AddClaimAsync(User user, string claimType, string value);
    Task<UserStreak> GetUserStreak(int id);
}
public class UserService(
    AppDbContext context,
    UserManager<User> userManager,
    RoleManager<ApplicationRole> roleManager,
    UserHandler userHandler,
    ICurrencyService currencyService,
    UserCurrencyHandler userCurrencyHandler,
    ICosmeticService cosmeticService,
    UserCosmeticHandler userCosmeticHandler,
    ICharacterService characterService,
    UserCharacterHandler userCharacterHandler,
    ITopicService topicService
    ) : IUserService
{
    /// <summary>
    /// Verify if a <see cref="User"/> exists
    /// </summary>
    /// <param name="id">The Id of the <see cref="User"/> to be verified</param>
    /// <exception cref="Exception">The user was not found</exception>
    public async Task VerifyUserExists(int id)
    {
        bool userExists = await context.Users.AnyAsync(u => u.Id == id);
        if (!userExists)
            throw new Exception("User not found");
    }

    /// <summary>
    /// Get a <see cref="User"/>
    /// </summary>
    /// <param name="id">The Id of the <see cref="User"/> to get</param>
    /// <returns>The User whose Id matches the given Id</returns>
    /// <exception cref="Exception">The User was not found</exception>
    public async Task<User> GetUser(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString())
            ?? throw new Exception("User not found");
        return user;
    }

    /// <summary>
    /// Get a <see cref="User"/> by their username
    /// </summary>
    /// <param name="username">The displayname of the <see cref="User"/> to get</param>
    /// <returns>The User whose displayname matches the given name</returns>
    /// <exception cref="KeyNotFoundException">A user with this display name was not found</exception>
    public async Task<User> GetUserByName(string username)
    {
        var user = await userManager.FindByNameAsync(username)
            ?? throw new KeyNotFoundException("No user found with this username");
        return user;
    }

    /// <summary>
    /// Get Several users
    /// </summary>
    /// <param name="range">The range of User Id's to get</param>
    /// <param name="offset">The offset of where to start</param>
    /// <returns>An <see cref="ICollection{T}"/> of <see cref="User"/>s</returns>
    /// <exception cref="Exception"></exception>
    public async Task<ICollection<User>> GetUsers(int range, int offset)
    {
        var users = await context.Users
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .Skip(offset)
                .Take(range)
                .ToListAsync();

        if (users.Count == 0)
            throw new Exception("No users found");

        return users;
    }

    /// <summary>
    /// Get the role of a given <see cref="User"/>
    /// </summary>
    /// <param name="user">The <see cref="User"/> to get the <see cref="Roles"/> from</param>
    /// <returns>The <see cref="Roles"/> of the <see cref="User"/></returns>
    public async Task<string> GetUserRole(User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return roles.FirstOrDefault() ?? Roles.User;
    }

    /// <summary>
    /// Checks for the required properties not to be a duplicate with another entry in the database.
    /// This prevents expected errors bubbling up in try catch statements
    /// </summary>
    /// <param name="ucr">The new record from which to check whether it has duplicate properties</param>
    /// <param name="original">Will not throw is the id of the found duplicate is equal to the original one</param>
    /// <exception cref="BadHttpRequestException">Is thrown when a duplicate entry is found
    /// along with which prop was duplicated.</exception>
    public async Task ThrowUserDuplicateProperty(UserCreateRecord ucr, User? original = null)
    {
        User? duplicate = await userManager.Users
            .Where(u => u.Email == ucr.Email || u.DisplayName == ucr.DisplayName)
            .FirstOrDefaultAsync();

        if (duplicate == null) return;
        if (original != null && duplicate.Id == original.Id) return;

        if (duplicate.Email == ucr.Email)
            throw new BadHttpRequestException("email_in_use", 409);

        if (duplicate.DisplayName == ucr.DisplayName)
            throw new BadHttpRequestException("name_in_use", 409);
    }

    /// <summary>
    /// Create a new User in the database
    /// </summary>
    /// <param name="ucr">The <see cref="UserCreateRecord"/> to create the new user from</param>
    /// <returns>The newly created <see cref="User"/></returns>
    /// <exception cref="Exception"></exception>
    public async Task<User> CreateUser(UserCreateRecord ucr)
    {
        await ThrowUserDuplicateProperty(ucr);

        var user = userHandler.CreateUserFromRecord(ucr);

        var result = await userManager.CreateAsync(user, ucr.Password);
        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description);

        if (!await roleManager.RoleExistsAsync(ucr.Role))
            throw new Exception($"Role '{ucr.Role}' does not exist.");

        if (!(await userManager.AddToRoleAsync(user, ucr.Role)).Succeeded)
            throw new Exception($"Could not add role '{ucr.Role}' to user");

        var defaultCos = await cosmeticService.GetDefaultCosmetics();
        var defaultCurs = await currencyService.GetCurrencies();
        var defaultChars = (await characterService.GetDefaultCharacters()).ToList();

        foreach (Currency cur in defaultCurs)
        {
            UserCurrency newUCur = userCurrencyHandler.NewUserCur(user, cur);
            context.UserCurrencies.Add(newUCur);
        }

        foreach (Cosmetic defCos in defaultCos)
        {
            UserCosmetic newUCos = userCosmeticHandler.NewUserCos(user, defCos);
            context.UserCosmetics.Add(newUCos);
        }

        foreach (Character defChar in defaultChars)
        {
            UserCharacter newUChar = userCharacterHandler.NewUserCharacter(user, defChar);
            context.UserCharacters.Add(newUChar);
        }
        await context.SaveChangesAsync();
        await context.UserCharacters
            .Where(uc => uc.UserId == user.Id && uc.CharacterId == defaultChars[user.Id % defaultChars.Count].Id)
            .ExecuteUpdateAsync(s => s.SetProperty(uc => uc.Selected, true));

        UserStreak newStreak = userHandler.CreateUserStreak(user);
        context.UserStreaks.Add(newStreak);

        await context.SaveChangesAsync();

        await topicService.DefaultUserProficiencies(user.Id);

        return user;
    }

    /// <summary>
    /// Update a User by their Id and a <see cref="UserCreateRecord"/>
    /// </summary>
    /// <param name="id">Id of the <see cref="User"/> to update</param>
    /// <param name="ucr">The <see cref="UserCreateRecord"/> to update the <see cref="User"/> from.</param>
    /// <returns>The updated <see cref="User"/></returns>
    /// <exception cref="Exception"></exception>
    public async Task<User> UpdateUser(int id, UserCreateRecord ucr)
    {
        var user = await GetUser(id);
        await ThrowUserDuplicateProperty(ucr, original: user);

        if (!await roleManager.RoleExistsAsync(ucr.Role))
            throw new Exception($"Role '{ucr.Role}' does not exist.");

        userHandler.UpdateUserFromRecord(user, ucr);

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new Exception(updateResult.Errors.First().Description);

        if (!string.IsNullOrEmpty(ucr.Password))
        {
            string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await userManager.ResetPasswordAsync(user, resetToken, ucr.Password);
            if (!passwordResult.Succeeded)
                throw new Exception(passwordResult.Errors.First().Description);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                throw new Exception(removeResult.Errors.First().Description);
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, ucr.Role);
        if (!addRoleResult.Succeeded)
            throw new Exception(addRoleResult.Errors.First().Description);

        return user;
    }

    /// <summary>
    /// Delete a user by their Id
    /// </summary>
    /// <param name="id">The Id of the <see cref="User"/> to delete</param>
    /// <exception cref="Exception"></exception>
    public async Task DeleteUser(int id)
    {

        var user = await GetUser(id);

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Any())
        {
            var roleRemovalResult = await userManager.RemoveFromRolesAsync(user, roles);
            if (!roleRemovalResult.Succeeded)
                throw new Exception(roleRemovalResult.Errors.First().Description);
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description);
    }

    /// <summary>
    /// Add a claim to a <see cref="User"/> only when the user does not have that claim yet.
    /// </summary>
    /// <param name="user">The <see cref="User"/> for whom to add a <see cref="Claim"/></param>
    /// <param name="claimType">The type of <see cref="Claim"/> to add, see <see cref="ClaimTypes"/></param>
    /// <param name="value">The value for the claim</param>
    /// <returns></returns>
    public async Task AddClaimAsync(User user, string claimType, string value)
    {
        try
        {
            IList<Claim> userClaims = await userManager.GetClaimsAsync(user);

            if (!userClaims.Any((claim) => claim.Type == claimType && claim.Value == value))
                await userManager.AddClaimAsync(user, new Claim(claimType, value));
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to add claim to user", ex);
        }
    }

    /// <summary>
    /// Get streak data for a user.
    /// </summary>
    /// <param name="id">The Id of the user for whom to retrieve the streak data.</param>
    /// <returns>The <see cref="UserStreak"/> of the specified user.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the streak data for the specified user is not found.</exception>
    public async Task<UserStreak> GetUserStreak(int id)
    {
        var streak = await context.UserStreaks.FirstOrDefaultAsync(s => s.UserId == id)
            ?? throw new KeyNotFoundException("Streak not found");
        return streak;
    }
}
