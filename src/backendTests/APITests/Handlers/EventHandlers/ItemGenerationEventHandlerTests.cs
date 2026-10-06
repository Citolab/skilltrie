/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.NewItemsAlgorithm;
using API.Handlers.EventHandlers;
using API.Services;
using API.Tools.EventQueue;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITests.Handlers.EventHandlers;

public class ItemGenerationEventHandlerTests
{
    private readonly Mock<IAIService> _aiService = new();
    private readonly ItemGenerationEventHandler _handler;

    public ItemGenerationEventHandlerTests()
    {
        _handler = new ItemGenerationEventHandler(_aiService.Object);
    }

    [Fact]
    public async Task ProcessAsync_WhenMultipleChoiceIsThree_CallsMakeQuestionsThreeTimes()
    {
        // Arrange
        var record = new NewItemsRecord { TopicId = 1, UserId = 1, TotalNewItems = 3, MultipleChoice = 3 };
        var eventData = new EventData(EventType.ItemGeneration, record);

        // Act
        await _handler.ProcessAsync(eventData, CancellationToken.None);

        // Assert
        _aiService.Verify(x => x.MakeQuestions(1, 3), Times.Exactly(1));
    }

    [Fact]
    public async Task ProcessAsync_WhenMultipleChoiceIsZero_NeverCallsMakeQuestions()
    {
        // Arrange
        var record = new NewItemsRecord { TopicId = 1, UserId = 1, MultipleChoice = 0 };
        var eventData = new EventData(EventType.ItemGeneration, record);

        // Act
        await _handler.ProcessAsync(eventData, CancellationToken.None);

        // Assert
        _aiService.Verify(x => x.MakeQuestions(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WhenWrongDataType_ThrowsArgumentException()
    {
        // Arrange
        var eventData = new EventData(EventType.ItemGeneration, "wrong type");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.ProcessAsync(eventData, CancellationToken.None));
    }

    [Fact]
    public void CanHandle_WhenItemGeneration_ReturnsTrue()
    {
        var result = _handler.CanHandle(EventType.ItemGeneration);
        Assert.True(result);
    }

    [Fact]
    public void CanHandle_WhenOtherEventType_ReturnsFalse()
    {
        var result = _handler.CanHandle((EventType)999);
        Assert.False(result);
    }
}
