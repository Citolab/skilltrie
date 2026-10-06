/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

/// <summary>
/// The Cosmetic model, represents a single cosmetic that <see cref="User"/> can own or purchase.
/// Cosmetics define visual customisations, such as clothing for avatars
/// Cosmetics are stored in a .riv file.
/// </summary>
[Table(nameof(Cosmetic))]
public class Cosmetic
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public int Price { get; set; }
    public Currency Currency { get; init; } = null!;
    public int CurrencyId { get; init; } = 1; // Change when multiple currencies are added
    public ClothingType ClothingType { get; init; }
    public string RiveFile { get; init; } = null!; // Currently the same for all avatar cosmetics
    public string RiveArtboard { get; init; } = null!;
    public string RiveStateMachine { get; init; } = null!;
    public string RiveInput { get; init; } = null!;
    public int RiveInputValue { get; init; }
    public bool DefaultOwned { get; init; } // Whether a User should get this cosmetic on user creation
    public string IconFile { get; init; } = null!; // Sprite location on the frontend
}

/// <summary>
/// The UserCosmetic model
/// Tracks the balance that a <see cref="Models.User"/> has of a <see cref="Models.Cosmetic"/>
/// </summary>
[Table(nameof(UserCosmetic))]
public class UserCosmetic
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public User User { get; init; } = null!;
    public int CosmeticId { get; init; }
    public Cosmetic Cosmetic { get; init; } = null!;
    public bool Equipped { get; set; }

}

/// <summary>
/// The type of a <see cref="Cosmetic"/>.
/// </summary>
public enum ClothingType
{
    Hair,
    Hat,
    Glasses,
    Shirt,
    Pants,
    Shoes,
}
