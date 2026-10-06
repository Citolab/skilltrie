/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers.EventHandlers;
using API.Tools.EventQueue;

namespace API.Services;

/// <summary>
/// Servive responsible for dequeuing the queued items from the <see cref="IEventQueue"/>. 
/// This class is ran in the background of the program and does this by making use of the <see cref="BackgroundService"/> class.
/// </summary>
/// <param name="queue">The queue which stores all the events.</param>
/// <param name="serviceProvider">A service provider used for creating a scope to avoid scope issues.</param>
public class EventQueueService(
    IEventQueue queue, 
    IServiceProvider serviceProvider
) : BackgroundService
{
    /// <summary>
    /// Asynchronously executes event processing by reading events from the queue and dispatching them to the
    /// appropriate handler.
    /// </summary>
    /// <remarks>This method processes all available events in the queue until the operation is canceled. Each
    /// event is dispatched to the first handler that can process its event type.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous execution operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no suitable handler is found for a given event type.</exception>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await foreach(EventData eventItem in queue.ReadAllAsync(cancellationToken))
        {
            using var scope = serviceProvider.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IEventHandler>();

            var availableHandlers = handlers.Where(x => x.CanHandle(eventItem.EventType)).ToList();
                    
            if (availableHandlers.Count == 0)
                throw new InvalidOperationException($"No handler found for {eventItem.EventType}");

            try
            {
                foreach (var handler in availableHandlers)
                {
                    await handler.ProcessAsync(eventItem, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                eventItem.Attempts += 1;

                if (eventItem.Attempts <= 3)
                    await queue.QueueAsync(eventItem, cancellationToken);

                Console.Error.WriteLine($"Failed to process eventItem corresponding to {eventItem.EventType}");
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}