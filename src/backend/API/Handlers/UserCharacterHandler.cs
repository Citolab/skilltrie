/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Handlers;

/// <summary>
/// The CharacterHandler defines the methods to edit objects of type <see cref="UserCharacter"/>.
/// </summary>
public class UserCharacterHandler
{
    /// <summary>
    /// Assigns a character to a User.
    /// </summary>
    /// <param name="user"> The user</param>
    /// <param name="character"> The character.</param>
    /// <returns>
    /// On success, a <see cref="UserCharacter"/> object.
    /// Throws an error if the user does not exist or already own the cosmetic.
    /// </returns>
    public UserCharacter NewUserCharacter(User user, Character character)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(character);

        return new UserCharacter()
        {
            UserId = user.Id,
            User = user,
            CharacterId = character.Id,
            Character = character,
            Selected = false
        };
    }
}