/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using API.Handlers;

namespace APITests.Handlers;

public class CurrencyHandlerTest
{
    private readonly  UserCurrencyHandler _userCurrencyHandler = new UserCurrencyHandler();

    [Fact(DisplayName = "NewUserCur throws exception when user is null")]
    public void NewUserCurThrowsExceptionWhenUserIsNull()
    {
        User user = null;
        Currency currency = new Currency { Id = 10 };

        Assert.Throws<ArgumentNullException>(() => _userCurrencyHandler.NewUserCur(user, currency));
    }

    [Fact(DisplayName = "NewUserCur throws exception when currency is null")]
    public void NewUserCurThrowsExceptionWhenCurIsNull()
    {
        User user = new User { Id = 1 };
        Currency currency = null;

        Assert.Throws<ArgumentNullException>(() => _userCurrencyHandler.NewUserCur(user, currency));
    }

    [Fact(DisplayName = "NewUserCur gives user currency on valid inputs")]
    public void NewUserCurGivesUserCurrencyOnValidInputs()
    {
        User user = new User { Id = 1 };
        Currency currency = new Currency { Id = 10, StartingAmount = 11 };

        var res = _userCurrencyHandler.NewUserCur(user, currency);

        Assert.NotNull(res);
        Assert.Equal(user.Id, res.UserId);
        Assert.Same(user, res.User);

        Assert.Equal(currency.Id, res.CurrencyId);
        Assert.Same(currency, res.Currency);

        Assert.Equal(currency.StartingAmount, res.Amount);
    }

    [Fact(DisplayName = "UpdateUserCur throws exception when balance becomes negative")]
    public void UpdateUserCurGivesNegativeBalance()
    {
        User user = new User { Id = 1 };
        Currency currency = new Currency { Id = 10, StartingAmount = 11 };
        UserCurrency userCur = new UserCurrency { User = user, Currency = currency };

        Assert.Throws<Exception>(() => _userCurrencyHandler.UpdateCurrency(userCur, -12));
    }

    [Fact(DisplayName = "UpdateUserCur gives correct value on valid inputs")]
    public void UpdateUserCurGivesCorrectValueOnValidInputs()
    {
        User user = new User { Id = 1 };
        Currency currency = new Currency { Id = 10, StartingAmount = 11 };
        int raiseAmmount = 12;
        UserCurrency userCur = new UserCurrency { User = user, Currency = currency, Amount = currency.StartingAmount};

        _userCurrencyHandler.UpdateCurrency(userCur, raiseAmmount);

        Assert.Equal(userCur.Amount, currency.StartingAmount + raiseAmmount);
    }

}
