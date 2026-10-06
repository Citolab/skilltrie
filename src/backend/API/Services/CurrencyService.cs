/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers;
using Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public interface ICurrencyService
{
    Task<Currency> GetCurrency(int currencyId);
    Task<ICollection<Currency>> GetCurrencies();
}

/// <summary>
/// The CurrencyHandler defines the methods to edit objects in the <see cref="Currency"/> and <see cref="UserCurrency"/> tables in the database.
/// </summary>
/// <param name="context">The database context</param>
public class CurrencyService(AppDbContext context) : ICurrencyService
{
    /// <summary>
    /// Gets a currency based on its identifier
    /// <param name="currencyId"> Identifier of the currency </param>
    /// </summary>
    /// <returns>
    /// On success, the task result contains a <see cref="Currency"/> instance .
    /// </returns>
    public async Task<Currency> GetCurrency(int currencyId)
    {
        var cur = await context.Currencies.FindAsync(currencyId)
            ?? throw new Exception($"Currency not found.");

        return cur;
    }

    /// <summary>
    /// Gets the currencies
    /// </summary>
    /// <returns>
    /// On success, the task result contains a collection of <see cref="Cosmetic"/> instances.
    /// </returns>
    public async Task<ICollection<Currency>> GetCurrencies()
    {
        var curs = await context.Currencies.ToListAsync();

        if (curs.Count == 0)
            throw new Exception("No currencies defined");

        return curs;
    }
}

public interface IUserCurrencyService
{
    Task<ICollection<UserCurrency>> GetUserCurrencies(int userId);
    Task<UserCurrency> UpdateUserCurrency(int userId, int currencyId, int amount);
}
public class UserCurrencyService(
    AppDbContext context,
    IUserService userService,
    ICurrencyService currencyService,
    UserCurrencyHandler userCurrencyHandler
    ) : IUserCurrencyService
{
    /// <summary>
    /// Gets the usercurrencies of a <see cref="User"/>
    /// <param name="userId"> Identifier of the user </param>
    /// </summary>
    /// <returns>
    /// On success, the task result contains a collection of <see cref="UserCurrency"/> instances.
    /// Throws an error if the user can not be found.
    /// </returns>
    public async Task<ICollection<UserCurrency>> GetUserCurrencies(int userId)
    {

        var user = await userService.GetUser(userId);

        var userCurs = await context.UserCurrencies
            .Where(ucs => ucs.UserId == userId)
            .ToListAsync();

        return userCurs;
    }

    /// <summary>
    /// Updates the balance of a user for a given currency
    /// <param name="userId"> Identifier of the user </param>
    /// <param name="currencyId"> Identifier of the currency </param>
    /// <param name="amount"> The amount to update the balance with, can be negative </param>
    /// </summary>
    /// <returns>
    /// On success, the task result contains a <see cref="UserCurrency"/> instance.
    /// Throws an error if the user/currency can not be found or if the action results in a negative balance.
    /// </returns>
    public async Task<UserCurrency> UpdateUserCurrency(int userId, int currencyId, int amount)
    {
        var user = await userService.GetUser(userId);
        var cur = await currencyService.GetCurrency(currencyId);

        var userCur = await context.UserCurrencies
            .FirstOrDefaultAsync(ucs => ucs.UserId == userId && ucs.CurrencyId == currencyId);

        if (userCur == null)
        {
            userCur = userCurrencyHandler.NewUserCur(user, cur);
            context.UserCurrencies.Add(userCur);
        }

        userCurrencyHandler.UpdateCurrency(userCur, amount);

        await context.SaveChangesAsync();
        return userCur;
    }
}


