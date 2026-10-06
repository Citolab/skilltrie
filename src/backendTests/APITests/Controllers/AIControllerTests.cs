/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers;
using Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.AspNetCore.Http;

namespace APITests.Controllers;

public class AIControllerTests
{
    private readonly Mock<IAIService> _mockAIService;
    private readonly AIController _controller;

    public AIControllerTests()
    {
        _mockAIService = new Mock<IAIService>();
        _controller = new AIController(_mockAIService.Object);
    }

    private static Item DummyItem(int id = 1, string questionText = "What is 2+2?")
        => new Item
        {
            Id = id,
            QuestionText = questionText,
            ResponseType = "conceptual",
            Type = Item.ItemType.MultipleChoice,
            Source = Item.ItemSource.LLM,
            Lang = Item.Language.English,
            AnswerExplanation = "2+2=4",
            AppearanceCount = 0,
            Level = "Easy"
        };

    [Fact]
    public async Task MakeQuestionsReturnsOk()
    {
        var item = DummyItem();
        _mockAIService.Setup(s => s.MakeQuestions(0, 1)).ReturnsAsync([item]);

        var result = await _controller.MakeQuestions();

        // assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returnedItem = Assert.IsType<Item>(ok.Value);
        Assert.Equal(item.Id, returnedItem.Id);
        Assert.Equal(item.QuestionText, returnedItem.QuestionText);
        Assert.Equal(item.ResponseType, returnedItem.ResponseType);
        Assert.Equal(item.Type, returnedItem.Type);
        Assert.Equal(item.Source, returnedItem.Source);
        Assert.Equal(item.Lang, returnedItem.Lang);
    }

    [Fact]
    public async Task MakeQuestionsReturnsProblemOnException()
    {
        // setup
        _mockAIService.Setup(s => s.MakeQuestions(0, 1)).ThrowsAsync(new Exception("AI failed"));

        // act
        var result = await _controller.MakeQuestions();

        // assert
        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problem.StatusCode);

        var details = Assert.IsType<ProblemDetails>(problem.Value);
        Assert.Equal("An Error occurred while creating a sample question", details.Detail);
    }

    [Fact]
    public async Task MakeQuestionWithIdReturnsOk()
    {
        var item = DummyItem(id: 5);
        _mockAIService.Setup(s => s.MakeQuestions(5, 1)).ReturnsAsync([item]);

        var result = await _controller.MakeQuestionsWithId(5);

        // assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returnedItem = Assert.IsType<Item>(ok.Value);
        Assert.Equal(item.Id, returnedItem.Id);
        Assert.Equal(item.QuestionText, returnedItem.QuestionText);
        Assert.Equal(item.ResponseType, returnedItem.ResponseType);
        Assert.Equal(item.Type, returnedItem.Type);
        Assert.Equal(item.Source, returnedItem.Source);
        Assert.Equal(item.Lang, returnedItem.Lang);
    }

    [Fact]
    public async Task MakeQuestionsWithIdReturnsProblemOnException()
    {
        // setup
        _mockAIService.Setup(s => s.MakeQuestions(5, 1)).ThrowsAsync(new Exception("AI failed"));

        // act
        var result = await _controller.MakeQuestionsWithId(5);

        // assert
        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, problem.StatusCode);

        var details = Assert.IsType<ProblemDetails>(problem.Value);
        Assert.Equal("An Error occurred while creating a sample question", details.Detail);
    }

    [Theory]
    [InlineData(5, 1)]
    [InlineData(5, 2)]
    [InlineData(5, 5)]
    public async Task MakeNumQuestionsWithIdReturnsOk(int topicId, int num)
    {
        // arrange
        List<Item> items = new();
        for (int i = 0; i < num; i++)
            items.Add(DummyItem(i));

        _mockAIService.Setup(s => s.MakeQuestions(topicId, num)).ReturnsAsync(items);

        // act
        var result = await _controller.MakeNumQuestions(topicId, num);

        // assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var returnedItems = Assert.IsType<List<Item>>(ok.Value);
        Assert.Equal(num, returnedItems.Count);
    }

    [Fact]
    public async Task MakeNumQuestionsWithIdReturnsProblemOnException()
    {
        // setup
        int id = 5;
        int num = 2;
        _mockAIService.Setup(s => s.MakeQuestions(id, num)).ThrowsAsync(new Exception("AI failed"));

        // act
        var result = await _controller.MakeNumQuestions(id, num);

        // assert
        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);

        var details = Assert.IsType<ProblemDetails>(problem.Value);
        Assert.Equal("An Error occurred while creating a sample question", details.Detail);
    }

    [Fact]
    public async Task CreateFeedBackReturnsTokensOnSuccess()
    {
        // arrange
        int testId = 1;
        var tokens = new List<string> { "Here ", "is ", "your ", "feedback." };
        _mockAIService.Setup(s => s.AiFeedback(testId)).Returns(ToAsyncEnumerable(tokens));

        var bodyStream = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = bodyStream;
        _controller.ControllerContext = new ControllerContext { HttpContext = context };

        // act
        await _controller.CreateFeedBack(testId);

        // assert
        bodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(bodyStream).ReadToEndAsync();
        Assert.Equal("Here is your feedback.", responseBody);
        Assert.Equal("text/plain", context.Response.ContentType);
    }

    [Fact]
    public async Task CreateFeedBackReturnsErrorMessageOnException()
    {
        // arrange
        int testId = 1;
        _mockAIService.Setup(s => s.AiFeedback(testId)).Returns(ThrowingAsyncEnumerable());

        var bodyStream = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = bodyStream;
        _controller.ControllerContext = new ControllerContext { HttpContext = context };

        // act
        await _controller.CreateFeedBack(testId);

        // assert
        bodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(bodyStream).ReadToEndAsync();
        Assert.Equal("partial An Error occurred while creating feedback", responseBody);
    }

    // helpers at the bottom of the class
    private static async IAsyncEnumerable<string> ToAsyncEnumerable(IEnumerable<string> items)
    {
        foreach (var item in items)
            yield return item;
        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<string> ThrowingAsyncEnumerable()
    {
        yield return "partial ";
        await Task.CompletedTask;
        throw new Exception("AI failed");
    }
}
