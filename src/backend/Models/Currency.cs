/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.ComponentModel.DataAnnotations.Schema;
namespace Models;

/// <summary>
/// The Currency model, represents a single Currency.
/// Also allows for assigning a starting amount on <see cref="User"/> creation.
/// </summary>
[Table(nameof(Currency))]
public class Currency
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Sprite { get; init; } = null!; // Sprite location on the frontend
    public int StartingAmount { get; init; } = 0; // Default value used when assigned to a User
}

/// <summary>
/// The UserCurrency model
/// Tracks the balance that a <see cref="Models.User"/> has of a <see cref="Models.Currency"/>
/// </summary>
[Table(nameof(UserCurrency))]
public class UserCurrency
{
    public int Id { get; init; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;
    public int Amount { get; set; }
}

