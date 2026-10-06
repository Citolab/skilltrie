/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Controllers.DTOs;

public class CharacterDTO
{
    public required int Id { get; set; }
    public required string Name { get; set; }

    public static CharacterDTO CreateCharacterDTO(Character character)
    {
        return new CharacterDTO
        {
            Id = character.Id,
            Name = character.Name
        };
    }
}

public class UserCharacterDTO
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required bool Selected { get; set; }
    public required string Palette { get; set; }

    public static UserCharacterDTO CreateUserCharacterDTO(UserCharacter character)
    {
        return new UserCharacterDTO
        {
            Id = character.Character.Id,
            Name = character.Character.Name,
            Selected = character.Selected,
            Palette = character.Palette
        };
    }
}