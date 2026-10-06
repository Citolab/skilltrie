/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

[Table(nameof(Character))]
public class Character
{
    public int Id { get; set; }

    public required string Name { get; set; }
}

[Table(nameof(UserCharacter))]
public class UserCharacter
{
    public int UserId { get; set; }

    public int CharacterId { get; set; }

    public bool Selected { get; set; } = false;

    public string Palette { get; set; } = "classic";

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(CharacterId))]
    public Character Character { get; set; } = null!;
}