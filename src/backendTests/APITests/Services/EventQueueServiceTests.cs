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
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITests.Services;

public class EventQueueServiceTests
{
    private readonly Mock<IEventQueue> _queue = new();
    private readonly Mock<IEventHandler> _handler = new();
    private readonly Mock<IServiceProvider> _serviceProvider = new();
    private readonly Mock<IServiceScope> _scope = new();
    private readonly Mock<IServiceScopeFactory> _scopeFactory = new();
    private readonly EventQueueService _service;

    public EventQueueServiceTests()
    {
        // Wire up the scope chain
        _scope.Setup(x => x.ServiceProvider).Returns(_serviceProvider.Object);
        _scopeFactory.Setup(x => x.CreateScope()).Returns(_scope.Object);
        _serviceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_scopeFactory.Object);
        _serviceProvider.Setup(x => x.GetService(typeof(IEnumerable<IEventHandler>)))
            .Returns(new List<IEventHandler> { _handler.Object });

        _service = new EventQueueService(_queue.Object, _serviceProvider.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEventQueued_CallsCorrectHandler()
    {
        // Arrange
        var eventData = new EventData(EventType.ItemGeneration, new NewItemsRecord() { TopicId = 1, UserId = 1});
        _queue.Setup(x => x.ReadAllAsync(It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable(eventData));
        _handler.Setup(x => x.CanHandle(EventType.ItemGeneration)).Returns(true);
        _handler.Setup(x => x.ProcessAsync(eventData, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _handler.Verify(x => x.ProcessAsync(eventData, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoHandlerFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventData = new EventData(EventType.ItemGeneration, new NewItemsRecord() { TopicId = 1, UserId = 1 });
        _queue.Setup(x => x.ReadAllAsync(It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable(eventData));
        _handler.Setup(x => x.CanHandle(It.IsAny<EventType>())).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleHandlers_CallsCorrectOne()
    {
        // Arrange
        var eventData = new EventData(EventType.ItemGeneration, new NewItemsRecord() { TopicId = 1, UserId = 1 });
        var wrongHandler = new Mock<IEventHandler>();
        _queue.Setup(x => x.ReadAllAsync(It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable(eventData));

        _handler.Setup(x => x.CanHandle(EventType.ItemGeneration)).Returns(true);
        wrongHandler.Setup(x => x.CanHandle(EventType.ItemGeneration)).Returns(false);

        _serviceProvider.Setup(x => x.GetService(typeof(IEnumerable<IEventHandler>)))
            .Returns(new List<IEventHandler> { _handler.Object, wrongHandler.Object });

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _handler.Verify(x => x.ProcessAsync(eventData, It.IsAny<CancellationToken>()), Times.Once);
        wrongHandler.Verify(x => x.ProcessAsync(It.IsAny<EventData>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static async IAsyncEnumerable<EventData> AsyncEnumerable(params EventData[] items)
    {
        foreach (var item in items)
            yield return item;
        await Task.CompletedTask;
    }
}