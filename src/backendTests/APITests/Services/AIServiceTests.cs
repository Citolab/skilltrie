/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AI;
using API.Handlers;
using Models;
using API.Services;
using API.Tools.AI;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using AI.FeedbackGen;
using AI.QuestionGen;
using Microsoft.Extensions.Logging;

namespace APITests.Services;

public class AIServiceTests
{
    private readonly AppDbContext db;
    private readonly AIService aiService;
    private readonly Mock<IAIAPI> aiApiMock;
    private readonly AIUtils aiUtils;
    private readonly OriginalFeedbackGen originalFeedbackGen;
    private readonly OriginalQuestionGen originalQuestionGen;

    public AIServiceTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        db = new AppDbContext(options);
        db.Database.EnsureCreated();

        aiUtils = new AIUtils();
        aiApiMock = new Mock<IAIAPI>();
        originalFeedbackGen = new OriginalFeedbackGen(aiApiMock.Object, aiUtils);
        originalQuestionGen = new OriginalQuestionGen(aiApiMock.Object, aiUtils);

        aiService = new AIService(db, originalFeedbackGen, originalQuestionGen, aiUtils, Mock.Of<ILogger<AIService>>());
    }

    [Fact]
    public async Task MakeQuestions_CallsAI_ParsesJson_SavesAndReturnsItem()
    {
        // Arrange: valid JSON matching feedback_schema.txt
        var json = """
        [
        {
        "Metainformation": {
            "item_type": "Multiple_choice",
            "response_type": "Conceptual",
            "language": "Dutch",
            "level": "Statistical Literacy"
        },
        "Question": "<p>John heeft de resultaten van zijn laatste toets terug gehad: [5.4, 6.7, 8.2, 5.9, 7.3, 6.5] Wat is het gemiddelde(mean) cijfer van John?</p>",
        "Options": [
            {"answer_text": "<p>6.50</p>", "correct": false},
            {"answer_text": "<p>6.67</p>", "correct": true},
            {"answer_text": "<p>7.00</p>", "correct": false},
            {"answer_text": "<p>6.70</p>", "correct": false}
        ],
        "Answer_explanation": "<p>Gemiddelde: 40 / 6 = 6.67. (het gemiddelde wordt berekend door de scores op te tellen en deze som te delen door het aantal deelnemers)</p>"
        }
        ]
        """;

        aiApiMock
            .Setup(a => a.PromptAI(It.IsAny<List<object>>(), 8000))
            .ReturnsAsync(json);

        db.Scopes.Add(new Scope { Id = 1, Name = "topic" });
        await db.SaveChangesAsync();

        // Act
        var result = await aiService.MakeQuestions(1, 1);

        // Assert: returned item
        Assert.NotNull(result);
        Assert.Equal("<p>John heeft de resultaten van zijn laatste toets terug gehad: [5.4, 6.7, 8.2, 5.9, 7.3, 6.5] Wat is het gemiddelde(mean) cijfer van John?</p>", result[0].QuestionText);

        // Assert: item saved to DB
        var saved = db.Items.Single();
        Assert.Equal(result[0].QuestionText, saved.QuestionText);
        Assert.Equal(Item.ItemType.MultipleChoice, saved.Type);

        // Assert: AI called exactly once
        aiApiMock.Verify(
            a => a.PromptAI(It.IsAny<List<object>>(), 8000),
            Times.Once
        );
    }

    [Fact]
    public async Task MakeQuestions_CallsAI_ParsesJson_SavesAndReturnsItem_MultipleQuestions()
    {
        // Arrange: valid JSON matching feedback_schema.txt
        var json = """
        [
        {
        "Metainformation": {
            "item_type": "Multiple_choice",
            "response_type": "Conceptual",
            "language": "Dutch",
            "level": "Statistical Literacy"
        },
        "Question": "<p>John heeft de resultaten van zijn laatste toets terug gehad: [5.4, 6.7, 8.2, 5.9, 7.3, 6.5] Wat is het gemiddelde(mean) cijfer van John?</p>",
        "Options": [
            {"answer_text": "<p>6.50</p>", "correct": false},
            {"answer_text": "<p>6.67</p>", "correct": true},
            {"answer_text": "<p>7.00</p>", "correct": false},
            {"answer_text": "<p>6.70</p>", "correct": false}
        ],
        "Answer_explanation": "<p>Gemiddelde: 40 / 6 = 6.67. (het gemiddelde wordt berekend door de scores op te tellen en deze som te delen door het aantal deelnemers)</p>"
        },
        {
        "Metainformation": {
            "item_type": "Multiple_choice",
            "response_type": "Conceptual",
            "language": "Dutch",
            "level": "Statistical Literacy"
        },
        "Question": "<p>Henk heeft de resultaten van zijn laatste toets terug gehad: [5.4, 6.7, 8.2, 5.9, 7.3, 6.5] Wat is het gemiddelde(mean) cijfer van Henk?</p>",
        "Options": [
            {"answer_text": "<p>7.00</p>", "correct": false},
            {"answer_text": "<p>6.67</p>", "correct": true},
            {"answer_text": "<p>6.70</p>", "correct": false},
            {"answer_text": "<p>6.50</p>", "correct": false}
        ],
        "Answer_explanation": "<p>Gemiddelde: 40 / 6 = 6.67. (het gemiddelde wordt berekend door de scores op te tellen en deze som te delen door het aantal deelnemers)</p>"
        },
        {
        "Metainformation": {
            "item_type": "Multiple_choice",
            "response_type": "Conceptual",
            "language": "Dutch",
            "level": "Statistical Literacy"
        },
        "Question": "<p>Pieter heeft de resultaten van zijn laatste toets terug gehad: [5.4, 6.7, 8.2, 5.9, 7.3, 6.5] Wat is het gemiddelde(mean) cijfer van Pieter?</p>",
        "Options": [
            {"answer_text": "<p>6.50</p>", "correct": false},
            {"answer_text": "<p>7.00</p>", "correct": false},
            {"answer_text": "<p>6.70</p>", "correct": false},
            {"answer_text": "<p>6.67</p>", "correct": true}
        ],
        "Answer_explanation": "<p>Gemiddelde: 40 / 6 = 6.67. (het gemiddelde wordt berekend door de scores op te tellen en deze som te delen door het aantal deelnemers)</p>"
        }
        ]
        """;

        aiApiMock
            .Setup(a => a.PromptAI(It.IsAny<List<object>>(), 8000))
            .ReturnsAsync(json);

        db.Scopes.Add(new Scope { Id = 1, Name = "topic" });
        await db.SaveChangesAsync();

        // Act
        var result = await aiService.MakeQuestions(1, 3);

        // Assert: returned item
        Assert.NotNull(result);
        Assert.Equal("<p>John heeft de resultaten van zijn laatste toets terug gehad: [5.4, 6.7, 8.2, 5.9, 7.3, 6.5] Wat is het gemiddelde(mean) cijfer van John?</p>", result[0].QuestionText);
        Assert.Equal("<p>Henk heeft de resultaten van zijn laatste toets terug gehad: [5.4, 6.7, 8.2, 5.9, 7.3, 6.5] Wat is het gemiddelde(mean) cijfer van Henk?</p>", result[1].QuestionText);
        Assert.Equal("<p>Pieter heeft de resultaten van zijn laatste toets terug gehad: [5.4, 6.7, 8.2, 5.9, 7.3, 6.5] Wat is het gemiddelde(mean) cijfer van Pieter?</p>", result[2].QuestionText);

        // Assert: item saved to DB
        var saved = db.Items.ToList();
        Assert.Equal(result[0].QuestionText, saved[0].QuestionText);
        Assert.Equal(Item.ItemType.MultipleChoice, saved[0].Type);
        Assert.Equal(result[1].QuestionText, saved[1].QuestionText);
        Assert.Equal(Item.ItemType.MultipleChoice, saved[1].Type);
        Assert.Equal(result[2].QuestionText, saved[2].QuestionText);
        Assert.Equal(Item.ItemType.MultipleChoice, saved[2].Type);

        // Assert: AI called exactly once
        aiApiMock.Verify(
            a => a.PromptAI(It.IsAny<List<object>>(), 8000),
            Times.Once
        );
    }

}