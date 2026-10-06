/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers;
using Models;
using API.Services;
using API.Tools.QTIConverting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using API.Tools.MathMLConverter;

namespace APITests.Controllers;

public class QTIControllerTests
{
    private readonly Mock<IItemService> _mockService;
    private readonly Mock<IMathMLConverter> _mockConverter;
    private readonly QTIController _controller;

    public QTIControllerTests()
    {
        _mockService = new Mock<IItemService>();
        _mockConverter = new Mock<IMathMLConverter>();
        _controller = new QTIController(_mockService.Object, _mockConverter.Object);
    }

    private static Item DummyItem()
    {
        return new Item
        {
            Id = 1,
            QuestionText = "<p>Question?</p>",
            Answers = new List<ItemAnswer>
            {
                new ItemAnswer { Id = 1, ItemId = 1, Correct = true, AnswerIdentifier = "1", AnswerText = "<p>Yup</p>", Chosen = 1 }
            }
        };
    }

    [Fact]
    public async Task GetItemReturns500OnFail()
    {
        // setup
        const int id = 0;
        _mockService.Setup(s => s.GetItem(id, true, true)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.GetItem(id);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetAssessmentReturnsAssessmentXML()
    {
        // setup
        Item[] returnedItems = [DummyItem()];
        int[] ids = returnedItems.Select(i => i.Id).ToArray();
        string returnedAssessmentXML = QTIXML.GenerateAssesment(returnedItems);
        _mockService
            .Setup(s => s.GetItemsForAssessment(It.IsAny<int[]>()))
            .ReturnsAsync(returnedItems);

        // act
        var result = await _controller.GetAssessment(ids);

        // assert
        var contentResult = Assert.IsType<ContentResult>(result.Result);
        Assert.Equal(returnedAssessmentXML, contentResult.Content);
    }

    [Fact]
    public async Task GetAssessmentReturns400OnInvalidInput()
    {
        // act
        var result = await _controller.GetAssessment([]);

        // assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAssessmentReturns500OnFail()
    {
        // setup
        Item[] returnedItems = [DummyItem()];
        int[] ids = returnedItems.Select(i => i.Id).ToArray();
        _mockService.Setup(s => s.GetItemsForAssessment(ids)).ThrowsAsync(new Exception());

        // act
        var result = await _controller.GetAssessment(ids);

        // assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetItems_WhenConversionFails_Returns500()
    {
        // setup
        Item[] returnedItems = [new Item { QuestionText = "<math>...</math>" }];
        int[] ids = returnedItems.Select(i => i.Id).ToArray();

        _mockService.Setup(r => r.GetItem(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(returnedItems[0]);

        _mockConverter.Setup(c => c.ConvertItemsToMathML(returnedItems))
            .ThrowsAsync(new InvalidOperationException("bad latex"));

        var result = await _controller.GetItem(ids[0]);

        var problemResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problemResult.StatusCode);
    }
}
