/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers.GameEventHandlers;
using Microsoft.EntityFrameworkCore;
using Models;

public interface ICharacterService
{
    Task<IEnumerable<Character>> GetCharacters();
    Task<UserCharacter> GetSelectedUserCharacter(int userId);
    Task<IEnumerable<UserCharacter>> GetUserCharacters(int userId);
    Task SetSelectedCharacter(int userId, int characterId, string palette);
    Task<ICollection<Character>> GetDefaultCharacters();
}

public class CharacterService(AppDbContext context, IGameEventManager gameEventManager) : ICharacterService
{

    /// <summary>
    /// Gets an enumerable of the <see cref="Character"/>s currently available in the database
    /// </summary>
    /// <exception cref="Exception">Throws when there are no characters populated in the database</exception>
    public async Task<IEnumerable<Character>> GetCharacters()
    {
        Character[] characters = await context.Characters.ToArrayAsync();

        if (characters.Length == 0)
            throw new Exception("No characters found");

        return characters;
    }

    /// <summary>
    /// Gets the <see cref="Character"/> that a specified user has set to selected
    /// </summary>
    /// <param name="userId">The id of the user for which to retrieve the selected <see cref="Character"/></param>
    /// <exception cref="KeyNotFoundException">Throws when the user has no character selected</exception>
    public async Task<UserCharacter> GetSelectedUserCharacter(int userId)
    {
        return await context.UserCharacters
            .Include(uc => uc.Character)
            .Where(uc => uc.UserId == userId && uc.Selected)
            .FirstOrDefaultAsync() ??
                throw new KeyNotFoundException("Could not find an selected character for this user");
    }

    /// <summary>
    /// Gets all the <see cref="Character"/>s the user has
    /// </summary>
    /// <param name="userId">The user</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<IEnumerable<UserCharacter>> GetUserCharacters(int userId)
    {
        var userCharacters = await context.UserCharacters
            .Include(uc => uc.Character)
            .Where(uc => uc.UserId == userId)
            .OrderBy(uc => uc.CharacterId)
            .ToArrayAsync();

        if (userCharacters.Length == 0) throw new KeyNotFoundException("No characters found for this user");

        return userCharacters;
    }

    /// <summary>
    /// Sets a <see cref="Character"/> for the specified user to selected whilst deselecting the other characters
    /// </summary>
    /// <param name="userId">The id of the user to set the character to selected</param>
    /// <param name="characterId">The id of the character the user has selected</param>
    /// <param name="palette">the name of the palette the selected character gets </param>
    public async Task SetSelectedCharacter(int userId, int characterId, string palette)
    {
        if (await context.Users.FindAsync(userId) == null)
            throw new KeyNotFoundException($"Could not find user with id: {userId}");
        if (await context.Characters.FindAsync(characterId) == null)
            throw new KeyNotFoundException($"Could not find character with id: {characterId}");

        await using var transaction = await context.Database.BeginTransactionAsync();

        var userCharacters = await context.UserCharacters
            .Where(uc => uc.UserId == userId)
            .ToListAsync();

        foreach (var uc in userCharacters)
            uc.Selected = false;

        await context.SaveChangesAsync();

        UserCharacter? target = userCharacters.FirstOrDefault(uc => uc.CharacterId == characterId && uc.UserId == userId);
        if (target is not null)
        {
            target.Selected = true;
            target.Palette = palette;
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        if (target != null) 
            await gameEventManager.TriggerGameEvent(new CharacterSelectedData {Character = target, UserId = userId});
    }

    public async Task<ICollection<Character>> GetDefaultCharacters()
    {
        var characters = await context.Characters.ToListAsync();

        if (characters.Count == 0) throw new Exception("No characters found");

        return characters;
    }
}