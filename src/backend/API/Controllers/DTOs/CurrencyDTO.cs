/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Controllers.DTOs;

/// <summary>
/// Data Transfer Object of <see cref="Currency"/>
/// Omits startingamount
/// </summary>
public record CurrencyDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Sprite { get; init; } = null!;

    /// <summary>
    /// Creates <see cref="CurrencyDto"/> from <see cref="Currency"/> object.
    /// </summary>
    public static CurrencyDto CreateCurrencyDto(Currency cur)
    {
        return new CurrencyDto()
        {
            Id = cur.Id,
            Name = cur.Name,
            Sprite = cur.Sprite,
        };
    }
}

/// <summary>
/// Data Transfer Object of <see cref="UserCurrency"/>
/// </summary>
public record UserCurrencyDto
{
    public int UserId { get; init; }
    public int CurrencyId { get; init; }
    public int Amount { get; set; }
    /// <summary>
    /// Creates <see cref="UserCurrencyDto"/> from <see cref="UserCurrency"/> object.
    /// </summary>
    public static UserCurrencyDto CreateUserCurrencyDto(UserCurrency uCur)
    {
        return new UserCurrencyDto()
        {
            UserId = uCur.UserId,
            CurrencyId = uCur.CurrencyId,
            Amount = uCur.Amount,
        };
    }

}

/// <summary>
/// Data Transfer Object of <see cref="UserCurrency"/>
/// Omits UserId as the user is identified via authorisation
/// </summary>
public record UserCurrencyUpdateDto
{
    public int CurrencyId { get; init; }
    public int Amount { get; init; }
}



