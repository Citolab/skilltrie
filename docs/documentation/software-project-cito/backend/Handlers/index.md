
# Handlers

Handlers are not a part of ASP .NET Core's *Model View Presenter* (MVP) paradigm. They are an extension to it, added on our behalf.
\
Their purpose was initially to abstract core application logic away from the [controllers](/backend/Controllers/index.md).
This kept the controllers small and digestable.

Over the course of time however, this solution proved inadequate to our needs. Therefore we added the [Services](/backend/Services/index.md) to our backend paradigm. Currently Handlers are simply an extension of services meant to abstract pure code from the services. This change was made in order to structurize the backend, and have clear seperation of responsability.

# Structure and Standard

A Handler class should be of the same name as the [service](/backend/Services/index.md) that makes use of it.

A handler class will contain a number of methods, which either return a new value based on any given number of inputs, or change one of the instances of data that it is given, in the case of models. These operations must be made pure, as to assist in readability of the services, and ease of unit testing.

# Example
```csharp
using backend.Models;

namespace backend.Handlers;

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
```

# Other handlers
As you may notice, inside the Handlers folder, there are multiple sub folders with handlers. These handlers don't follow the convention described above and should be seen as something different with coincidentally the same name. These handlers handle the what to do in certain situations.

For example, the BadgeHandlers are handlers that define and execute the functionality for when a specific badge.
The EventHandlers are handlers used by the Event Queue to resolve those events. These contain various events like a logger, or generating new items.

Admittedly this might cause some confusion for new developers using the codebase, but since these are also classes that handle certain functionality for different parts of the application, we have decided to still name them handlers and place them inside the Handlers folder.