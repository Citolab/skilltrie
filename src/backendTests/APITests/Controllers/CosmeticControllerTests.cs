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
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace APITests.Controllers;

public class CosmeticControllerTests
{
    private readonly Mock<ICosmeticService> _mockCosmeticService;
    private readonly Mock<IUserCosmeticService> _mockUserCosmeticService;
    private readonly CosmeticController _controller;

    public CosmeticControllerTests()
    {
        _mockCosmeticService = new Mock<ICosmeticService>();
        _mockUserCosmeticService = new Mock<IUserCosmeticService>();

        _controller = new CosmeticController(
            _mockCosmeticService.Object,
            _mockUserCosmeticService.Object
        );
    }

    private void SetUser(int userId, bool isAdmin = false)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, Roles.Admin));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
    }

    private static Cosmetic DummyCosmetic(int id, ClothingType type)
        => new Cosmetic { Id = id, Name = $"Cosmetic{id}", ClothingType = type, Price = 100 };

    private static UserCosmetic DummyUserCosmetic(int userId, int cosmeticId, bool equipped = false)
        => new UserCosmetic { UserId = userId, CosmeticId = cosmeticId, Equipped = equipped, Cosmetic = DummyCosmetic(cosmeticId, ClothingType.Hat) };

    [Fact]
    public async Task GetCosmeticsWithClothingTypeUsesType()
    {
        // setup
        var list = new List<Cosmetic> { DummyCosmetic(1, ClothingType.Hat) };
        _mockCosmeticService.Setup(s => s.GetCosmetics(ClothingType.Hat)).ReturnsAsync(list);

        // act
        var result = await _controller.GetCosmetics("Hat");

        // assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<CosmeticDto>>(ok.Value!);
        Assert.Single(dtos);
    }

    [Fact]
    public async Task GetCosmeticsWithoutClothingTypeUsesElseBranch()
    {
        // setup
        var list = new List<Cosmetic> { DummyCosmetic(1, ClothingType.Hat) };
        _mockCosmeticService.Setup(s => s.GetCosmetics(null)).ReturnsAsync(list);

        // act
        var result = await _controller.GetCosmetics();

        // assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<CosmeticDto>>(ok.Value!);
        Assert.Single(dtos);
    }

    [Fact]
    public async Task GetCosmeticsReturnsProblemOnException()
    {
        // setup
        _mockCosmeticService.Setup(s => s.GetCosmetics(null)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.GetCosmetics();

        // assert
        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task GetUserCosmeticsWithClothingTypeUsesType()
    {
        // setup
        SetUser(1);
        var list = new List<UserCosmetic> { DummyUserCosmetic(1, 1) };
        _mockUserCosmeticService.Setup(s => s.GetUserCosmetics(1, null, ClothingType.Shirt)).ReturnsAsync(list);

        // act
        var result = await _controller.GetUserCosmetics(null, "Shirt");

        // assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<UserCosmeticDto>>(ok.Value!);
        Assert.Single(dtos);
    }

    [Fact]
    public async Task GetUserCosmeticsWithoutClothingTypeUsesElseBranch()
    {
        // setup
        SetUser(1);
        var list = new List<UserCosmetic> { DummyUserCosmetic(1, 1) };
        _mockUserCosmeticService.Setup(s => s.GetUserCosmetics(1, null, null)).ReturnsAsync(list);

        // act
        var result = await _controller.GetUserCosmetics(null, null);

        // assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<UserCosmeticDto>>(ok.Value!);
        Assert.Single(dtos);
    }

    [Fact]
    public async Task GetUserCosmeticsReturnsUnauthorizedWhenTargetUserAndNotAdmin()
    {
        // setup
        SetUser(1);

        // act
        var result = await _controller.GetUserCosmetics(null, null, 2);

        // assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetUserCosmeticsReturnsProblemOnException()
    {
        // setup
        SetUser(1);
        _mockUserCosmeticService.Setup(s => s.GetUserCosmetics(1, null, null)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.GetUserCosmetics(null, null);

        // assert
        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task BuyUserCosmeticReturnsNoContent()
    {
        // setup
        SetUser(1);
        _mockUserCosmeticService.Setup(s => s.BuyUserCosmetic(1, 1)).ReturnsAsync(DummyUserCosmetic(1, 1));

        // act
        var result = await _controller.BuyUserCosmetic(new UserCosmeticDto { UserId = 1, CosmeticId = 1 });

        // assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task BuyUserCosmeticReturnsProblemOnException()
    {
        // setup
        SetUser(1);
        _mockUserCosmeticService.Setup(s => s.BuyUserCosmetic(1, 1)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.BuyUserCosmetic(new UserCosmeticDto { UserId = 1, CosmeticId = 1 });

        // assert
        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task EquipUserCosmeticReturnsNoContent()
    {
        // setup
        SetUser(1);
        var dto = new UserCosmeticDto { UserId = 1, CosmeticId = 1, Equipped = true };

        // act
        var result = await _controller.EquipUserCosmetic(dto);

        // assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task EquipUserCosmeticReturnsProblemOnException()
    {
        // setup
        SetUser(1);
        _mockUserCosmeticService.Setup(s => s.EquipUserCosmetic(1, 1, true)).ThrowsAsync(new Exception());
        var dto = new UserCosmeticDto { UserId = 1, CosmeticId = 1, Equipped = true };

        // act
        var result = await _controller.EquipUserCosmetic(dto);

        // assert
        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task SellUserCosmeticReturnsNoContent()
    {
        // setup
        SetUser(1);
        var dto = new UserCosmeticDto { UserId = 1, CosmeticId = 1 };

        // act
        var result = await _controller.SellUserCosmetic(dto);

        // assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task SellUserCosmeticReturnsProblemOnException()
    {
        // setup
        SetUser(1);
        _mockUserCosmeticService.Setup(s => s.SellUserCosmetic(1, 1)).ThrowsAsync(new Exception());
        var dto = new UserCosmeticDto { UserId = 1, CosmeticId = 1 };

        // act
        var result = await _controller.SellUserCosmetic(dto);

        // assert
        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task GetUserCosmeticsHitsReturnErrorWhenNotAdminAccessingOtherUser()
    {
        // setup
        SetUser(1);

        // act
        var result = await _controller.GetUserCosmetics(null, null, userId: 2);

        // assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task BuyUserCosmeticHitsReturnErrorWhenNotAdminBuyingForOtherUser()
    {
        // setup
        SetUser(1);
        var dto = new UserCosmeticDto { UserId = 2, CosmeticId = 1 };

        // act
        var result = await _controller.BuyUserCosmetic(dto);

        // assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task EquipUserCosmeticHitsReturnErrorWhenNotAdminEquippingOtherUser()
    {
        // setup
        SetUser(1);
        var dto = new UserCosmeticDto { UserId = 2, CosmeticId = 1, Equipped = true };

        // act
        var result = await _controller.EquipUserCosmetic(dto);

        // assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task SellUserCosmeticHitsReturnErrorWhenNotAdminSellingForOtherUser()
    {
        // setup
        SetUser(1);
        var dto = new UserCosmeticDto { UserId = 2, CosmeticId = 1 };

        // act
        var result = await _controller.SellUserCosmetic(dto);

        // assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}
