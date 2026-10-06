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

namespace APITests.Controllers;

public class ItemControllerTests
{
    private readonly Mock<IItemService> _mockService;
    private readonly ItemController _controller;

    private const int MaxQuestionBodyLength = 100;
    private const string column = "id";
    private const string order = "ascending";

    public ItemControllerTests()
    {
        _mockService = new Mock<IItemService>();
        _controller = new ItemController(_mockService.Object);
    }

    private static Item DummyItem()
    {
        return new Item();
    }

    private static SmallItemDto DummySmallItemDto()
    {
        return new SmallItemDto
        {
            Id = 0,
            Active = false,
            Type = Item.ItemType.MultipleChoice,
            Source = Item.ItemSource.LLM,
            Lang = Item.Language.English,
            QuestionText = "",
            AppearanceCount = 0,
            ResponseType = "",
            Level = null,
            Topics = [],
        };
    }

    private static ItemDto DummyItemDto()
    {
        return new ItemDto
        {
            Id = 0,
            Active = false,
            Type = Item.ItemType.MultipleChoice,
            Source = Item.ItemSource.LLM,
            Lang = Item.Language.English,
            AnswerExplanation = null,
            QuestionText = "",
            AppearanceCount = 0,
            ResponseType = "string",
            Level = null,
            Scopes = [],
            Answers = [],
            Reports = []
        };
    }

    [Fact]
    public async Task GetItemsReturnsSmallItemDtos()
    {
        // setup
        const int offset = 0;
        const int range = 5;

        Item[] returnedItems = [DummyItem()];
        var returnedNodePath = new Dictionary<int, string>();
        _mockService.Setup(s => s.GetItems(offset, range, MaxQuestionBodyLength, column, order)).ReturnsAsync(returnedItems);

        // act
        var result = await _controller.GetItems(offset, range);

        // assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var sentSmallItemDtos = Assert.IsAssignableFrom<ICollection<SmallItemDto>>(okObjectResult.Value);
        Assert.Equal(sentSmallItemDtos.Count, returnedItems.Length);
    }

    [Fact]
    public async Task GetItemsReturns500OnFail()
    {
        // setup
        const int offset = 0;
        const int range = 5;

        _mockService.Setup(s => s.GetItems(offset, range, MaxQuestionBodyLength, column, order)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.GetItems(offset, range);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetItemReturnsItem()
    {
        // setup
        const int id = 1;
        var returnedItem = DummyItem();
        _mockService.Setup(s => s.GetItem(1, true, true)).ReturnsAsync(returnedItem);

        // act
        var result = await _controller.GetItem(id);

        // assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var sentItem = Assert.IsAssignableFrom<ItemDto>(okObjectResult.Value);
        Assert.Equal(sentItem, ItemDto.CreateItemDto(returnedItem));
    }

    [Fact]
    public async Task GetItemReturns500OnFail()
    {
        // setup
        const int id = 1;
        _mockService.Setup(s => s.GetItem(id, true, true)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.GetItem(id);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdateItemCallsUpdateItem()
    {
        // setup
        var dto = DummyItemDto();
        var item = ItemDto.FromItemDto(dto);
        _mockService.Setup(s => s.UpdateItem(It.IsAny<Item>())).ReturnsAsync(item);

        // act
        var result = await _controller.UpdateItem(dto);

        // assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var sentItem = Assert.IsAssignableFrom<Item>(okObjectResult.Value);
        Assert.Equal(sentItem, item);
        _mockService.Verify(s => s.UpdateItem(It.IsAny<Item>()), Times.Once);
    }

    [Fact]
    public async Task UpdateItemReturns500OnFail()
    {
        // setup
        var dto = DummyItemDto();
        var item = ItemDto.FromItemDto(dto);
        _mockService.Setup(s => s.UpdateItem(It.IsAny<Item>())).ThrowsAsync(new Exception());

        // act
        var result = await _controller.UpdateItem(dto);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task SearchByQuestionTextReturnsItems()
    {
        // setup
        const string text = "dummy_search_text";
        SmallItemDto[] returnedItems = [DummySmallItemDto()];
        _mockService.Setup(s => s.GetItemByAnswertext(text, MaxQuestionBodyLength)).ReturnsAsync(returnedItems);

        // act
        var result = await _controller.SearchByQuestionText(text);

        // assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
        var sentItems = Assert.IsAssignableFrom<ICollection<SmallItemDto>>(okObjectResult.Value);
        Assert.Equal(sentItems, returnedItems);
    }

    [Fact]
    public async Task SearchByQuestionTextReturns500OnFail()
    {
        // setup
        const string text = "dummy_search_text";
        _mockService.Setup(s => s.GetItemByAnswertext(text, MaxQuestionBodyLength)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.SearchByQuestionText(text);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeactivateItemCallsActivateItem()
    {
        // setup
        const int id = 1;

        // act
        var result = await _controller.DeactivateItem(id);

        // assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeactivateItem(id), Times.Once);
    }

    [Fact]
    public async Task DeactivateItemReturns500OnFail()
    {
        // setup
        const int id = 1;
        _mockService.Setup(s => s.DeactivateItem(id)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.DeactivateItem(id);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task ActivateItemCallsActivateItem()
    {
        // setup
        const int id = 1;

        // act
        var result = await _controller.ReactivateItem(id);

        // assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.ActivateItem(id), Times.Once);
    }

    [Fact]
    public async Task ActivateItemReturns500OnFail()
    {
        // setup
        const int id = 1;
        _mockService.Setup(s => s.ActivateItem(id)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.ReactivateItem(id);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
