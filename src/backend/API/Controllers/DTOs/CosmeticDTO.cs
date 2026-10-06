/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Controllers.DTOs;

/// <summary>
/// Data Transfer Object of <see cref="Cosmetic"/>
/// Omits defaultowned
/// </summary>
public record CosmeticDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public int Price { get; set; }
    public int CurrencyId { get; init; } = 1;
    public string ClothingType { get; init; } = null!;
    public string RiveFile { get; init; } = null!;
    public string RiveArtboard { get; init; } = null!;
    public string RiveStateMachine { get; init; } = null!;
    public string RiveInput { get; init; } = null!;
    public int RiveInputValue { get; init; }
    public string IconFile { get; init; } = null!;

    /// <summary>
    /// Creates <see cref="CosmeticDto"/> from <see cref="Cosmetic"/> object.
    /// </summary>
    public static CosmeticDto CreateCosmeticDto(Cosmetic cos)
    {
        return new CosmeticDto
        {
            Id = cos.Id,
            Name = cos.Name,
            Price = cos.Price,
            ClothingType = cos.ClothingType.ToString(),
            CurrencyId = cos.CurrencyId,
            RiveFile = cos.RiveFile,
            RiveArtboard = cos.RiveArtboard,
            RiveInput = cos.RiveInput,
            RiveInputValue = cos.RiveInputValue,
            IconFile = cos.IconFile
        };
    }

}

/// <summary>
/// Data Transfer Object of <see cref="UserCosmetic"/>
/// </summary>
public record UserCosmeticDto
{
    public int? UserId { get; init; }
    public int CosmeticId { get; init; }
    public bool Equipped { get; set; }

    /// <summary>
    /// Creates <see cref="UserCosmeticDto"/> from <see cref="UserCosmetic"/> object.
    /// </summary>
    public static UserCosmeticDto CreateUserCosmeticDto(UserCosmetic uCos)
    {
        return new UserCosmeticDto()
        {
            UserId = uCos.UserId,
            CosmeticId = uCos.CosmeticId,
            Equipped = uCos.Equipped,
        };
    }
}



