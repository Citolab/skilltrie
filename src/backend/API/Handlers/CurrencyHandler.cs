/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Handlers;

public class UserCurrencyHandler
{
    /// <summary>
    /// Create a new UserCurrency object.
    /// </summary>
    /// <param name="user">User which's data is to be added</param>
    /// <param name="currency">Currency which's data is to be added</param>
    /// <returns>A new UserCurrency object with the needed data from the given <see cref="User"/> and <see cref="Currency"/> objects.</returns>
    public UserCurrency NewUserCur(User user, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(currency);

        return new UserCurrency()
        {
            UserId = user.Id,
            User = user,
            CurrencyId = currency.Id,
            Currency = currency,
            Amount = currency.StartingAmount
        };
    }
    /// <summary>
    /// Update a UserCurrency object
    /// </summary>
    /// <param name="uCur">The UserCurrency object to update</param>
    /// <param name="amount">The amount by which to increment the currency</param>
    /// <exception cref="Exception">Exception is thrown when a negative balance would be the result of the operation</exception>
    public void UpdateCurrency(UserCurrency uCur, int amount)
    {
        int newAmount = uCur.Amount + amount;
        if (newAmount < 0)
            throw new Exception("Negative balance not possible");

        uCur.Amount = newAmount;
    }
}
