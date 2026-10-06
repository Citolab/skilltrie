/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Handlers;

/// <summary>
/// The CosmeticHandler defines the methods to edit objects of type <see cref="UserCosmetic"/>.
/// </summary>
public class UserCosmeticHandler
{
    /// <summary>
    /// Assigns a cosmetic to a User.
    /// </summary>
    /// <param name="user"> The user</param>
    /// <param name="cos"> The cosmetic.</param>
    /// <returns>
    /// On success, a <see cref="UserCosmetic"/> object.
    /// Throws an error if the user does not exist or already own the cosmetic.
    /// </returns>
    public UserCosmetic NewUserCos(User user, Cosmetic cos)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(cos);

        return new UserCosmetic()
        {
            UserId = user.Id,
            User = user,
            CosmeticId = cos.Id,
            Cosmetic = cos,
            Equipped = false
        };
    }

    /// <summary>
    /// Equips or unequip a cosmetic for a user
    /// </summary>
    /// <param name="target"> The usercosmetic to be equipped</param>
    /// <param name="sameTypeCosmetics"> Cosmetics of the same type as the target owned by the user"/></param>
    /// <param name="equipped"> equip or unequip flag</param>
    /// <returns>
    /// On success, void.
    /// </returns>
    public void EquipCosmetic(UserCosmetic target, IEnumerable<UserCosmetic> sameTypeCosmetics, bool equipped)
    {
        foreach (var c in sameTypeCosmetics)
            c.Equipped = false;

        target.Equipped = equipped;
    }

    /// <summary>
    /// Logic for deleting a usercosmetic.
    /// </summary>
    /// <param name="userCosmetic"> The usercosmetic to be deleted</param>
    /// <returns>
    /// On success, void.
    /// </returns>
    public void PrepareForDeletion(UserCosmetic userCosmetic)
    {
        if (userCosmetic.Equipped)
        {
            userCosmetic.Equipped = false;
        }
    }
}
