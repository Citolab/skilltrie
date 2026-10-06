/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers;
using API.Controllers.DTOs;
using Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace APITests.Controllers;

public class CurrencyControllerTests
{
    private readonly Mock<ICurrencyService> _mockCurrencyService;
    private readonly Mock<IUserCurrencyService> _mockUserCurrencyService;
    private readonly CurrencyController _controller;

    public CurrencyControllerTests()
    {
        _mockCurrencyService = new Mock<ICurrencyService>();
        _mockUserCurrencyService = new Mock<IUserCurrencyService>();
        _controller = new CurrencyController(_mockCurrencyService.Object, _mockUserCurrencyService.Object);
    }

    private static Currency DummyCurrency(int id)
    {
        return new Currency
        {
            Id = id,
            Name = $"DummyCurrency_{id}",
            Sprite = $"DummyCurrency_{id}.png",
            StartingAmount = 0
        };
    }

    private void SetUser(int userId, bool isAdmin = false)
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
    };

        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, Roles.Admin));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    private UserCurrency DummyUserCurrency(int userId, int currencyId, int amount)
    {
        return new UserCurrency
        {
            UserId = userId,
            CurrencyId = currencyId,
            Amount = amount
        };
    }



    [Fact]
    public async Task GetCurrenciesReturnsCurrencies()
    {
        // setup
        var currencies = new List<Currency>
        {
            DummyCurrency(1),
            DummyCurrency(2)
        };

        var currencyDtos =
            currencies.Select(CurrencyDto.CreateCurrencyDto);

        _mockCurrencyService
            .Setup(s => s.GetCurrencies())
            .ReturnsAsync(currencies);

        // act
        var result = await _controller.GetCurrencies();

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtoList = Assert.IsAssignableFrom<IEnumerable<CurrencyDto>>(okResult.Value).ToList();

        Assert.Equal(currencies.Count, dtoList.Count);

        for (int i = 0; i < currencies.Count; i++)
        {
            var currency = currencies[i];
            var dto = dtoList[i];

            Assert.Equal(currency.Id, dto.Id);
            Assert.Equal(currency.Name, dto.Name);
            Assert.Equal(currency.Sprite, dto.Sprite);
        }
    }

    [Fact]
    public async Task GetCurrenciesReturnsEmptyListWhenNoCurrenciesExist()
    {
        // setup
        var currencies = new List<Currency>();

        _mockCurrencyService
            .Setup(s => s.GetCurrencies())
            .ReturnsAsync(currencies);

        // act
        var result = await _controller.GetCurrencies();

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtoList = Assert.IsAssignableFrom<IEnumerable<CurrencyDto>>(okResult.Value);

        Assert.Empty(dtoList);
    }

    [Fact]
    public async Task GetCurrenciesReturnsProblemWhenServiceThrows()
    {
        // setup
        _mockCurrencyService
            .Setup(s => s.GetCurrencies())
            .ThrowsAsync(new Exception("Database error."));

        // act
        var result = await _controller.GetCurrencies();

        // assert
        var problemResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problemResult.StatusCode);
    }


    [Fact]
    public async Task GetCurrencyReturnsCurrency()
    {
        // setup
        var currency = DummyCurrency(1);

        _mockCurrencyService
            .Setup(s => s.GetCurrency(currency.Id))
            .ReturnsAsync(currency);

        // act
        var result = await _controller.GetCurrency(currency.Id);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CurrencyDto>(okResult.Value);

        Assert.Equal(currency.Id, dto.Id);
        Assert.Equal(currency.Name, dto.Name);
        Assert.Equal(currency.Sprite, dto.Sprite);
    }

    [Fact]
    public async Task GetCurrencyReturnsProblemWhenNotFound()
    {
        // setup
        int id = 123;

        _mockCurrencyService
            .Setup(s => s.GetCurrency(id))
            .ThrowsAsync(new Exception("Currency not found."));

        // act
        var result = await _controller.GetCurrency(id);

        // assert
        var problemResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task GetUserCurrenciesReturnsCurrenciesForCurrentUser()
    {
        // setup
        SetUser(userId: 1, isAdmin: false);

        var currencies = new List<UserCurrency>
        {
            DummyUserCurrency(userId: 1, currencyId: 1, amount: 100),
            DummyUserCurrency(userId: 1, currencyId: 2, amount: 200)
        };

        _mockUserCurrencyService
            .Setup(s => s.GetUserCurrencies(1))
            .ReturnsAsync(currencies);

        // act
        var result = await _controller.GetUserCurrencies();

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtoList = Assert.IsAssignableFrom<IEnumerable<UserCurrencyDto>>(okResult.Value).ToList();

        Assert.Equal(2, dtoList.Count);

        for (int i = 0; i < currencies.Count; i++)
        {
            var currency = currencies[i];
            var dto = dtoList[i];

            Assert.Equal(currency.CurrencyId, dto.CurrencyId);
            Assert.Equal(currency.UserId, dto.UserId);
            Assert.Equal(currency.Amount, dto.Amount);
        }
    }

    [Fact]
    public async Task GetUserCurrenciesReturnsUnauthorizedWhenNotAdmin()
    {
        // setup
        SetUser(userId: 1, isAdmin: false);

        // act
        var result = await _controller.GetUserCurrencies(userId: 2);

        // assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetUserCurrenciesReturnsCurrenciesForOtherUserWhenAdmin()
    {
        // setup
        SetUser(userId: 1, isAdmin: true);

        var currencies = new List<UserCurrency>
        {
            DummyUserCurrency(userId: 2, currencyId: 1, amount: 300)
        };

        _mockUserCurrencyService
            .Setup(s => s.GetUserCurrencies(2))
            .ReturnsAsync(currencies);

        // act
        var result = await _controller.GetUserCurrencies(userId: 2);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtoList = Assert.IsAssignableFrom<IEnumerable<UserCurrencyDto>>(okResult.Value);

        Assert.Single(dtoList);
    }

    [Fact]
    public async Task GetUserCurrenciesReturnsProblemOnException()
    {
        // setup
        SetUser(userId: 1);

        _mockUserCurrencyService
            .Setup(s => s.GetUserCurrencies(1))
            .ThrowsAsync(new Exception("DB error"));

        // act
        var result = await _controller.GetUserCurrencies();

        // assert
        var problemResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUserCurrencyReturnsUpdatedCurrency()
    {
        // setup
        SetUser(userId: 1);

        var updateDto = new UserCurrencyUpdateDto
        {
            CurrencyId = 1,
            Amount = 50
        };

        var updatedCurrency = DummyUserCurrency(userId: 1, currencyId: 1, amount: 150);

        _mockUserCurrencyService
            .Setup(s => s.UpdateUserCurrency(1, 1, 50))
            .ReturnsAsync(updatedCurrency);

        // act
        var result = await _controller.UpdateUserCurrency(updateDto);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<UserCurrencyDto>(okResult.Value);

        Assert.Equal(updatedCurrency.Amount, dto.Amount);
    }

    [Fact]
    public async Task UpdateUserCurrencyReturnsUnauthorizedWhenNotAdmin()
    {
        // setup
        SetUser(userId: 1);

        var updateDto = new UserCurrencyUpdateDto
        {
            CurrencyId = 1,
            Amount = 50
        };

        // act
        var result = await _controller.UpdateUserCurrency(updateDto, userId: 2);

        // assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task UpdateUserCurrencyUpdatesOtherUserWhenAdmin()
    {
        // setup
        SetUser(userId: 1, isAdmin: true);

        var updateDto = new UserCurrencyUpdateDto
        {
            CurrencyId = 1,
            Amount = 100
        };

        var updated = DummyUserCurrency(userId: 2, currencyId: 1, amount: 200);

        _mockUserCurrencyService
            .Setup(s => s.UpdateUserCurrency(2, 1, 100))
            .ReturnsAsync(updated);

        // act
        var result = await _controller.UpdateUserCurrency(updateDto, userId: 2);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<UserCurrencyDto>(okResult.Value);

        Assert.Equal(updated.Amount, dto.Amount);
    }

    [Fact]
    public async Task UpdateUserCurrencyReturnsProblemOnException()
    {
        // setup
        SetUser(userId: 1);

        var updateDto = new UserCurrencyUpdateDto
        {
            CurrencyId = 1,
            Amount = 50
        };

        _mockUserCurrencyService
            .Setup(s => s.UpdateUserCurrency(1, 1, 50))
            .ThrowsAsync(new Exception("DB error"));

        // act
        var result = await _controller.UpdateUserCurrency(updateDto);

        // assert
        var problemResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problemResult.StatusCode);
    }

}
