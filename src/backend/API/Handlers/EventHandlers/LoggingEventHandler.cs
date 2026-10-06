/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.NewItemsAlgorithm;
using API.Tools.EventQueue;

namespace API.Handlers.EventHandlers;

/// <summary>
/// Concrete implementation of the handler used for item generation. 
/// </summary>
/// <param name="logger">Logger used for logging events</param>
public class LoggingEventHandler(ILogger<EventData> logger) : IEventHandler
{
    /// <summary>
    /// Indicates whether the event handler can handle the provided event
    /// </summary>
    /// <param name="eventType">The event that is being tried to handle</param>
    /// <returns>True whenever the current handler can handle the provided event</returns>
    public bool CanHandle(EventType eventType)
        => true;

    /// <summary>
    /// Processes the specified event data asynchronously by logging the event to the console.
    /// </summary>
    /// <param name="data">The event data to process.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    public async Task ProcessAsync(EventData data, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Executing {data.EventType} event with the following data {data.Data}");
    }
}