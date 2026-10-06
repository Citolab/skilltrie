/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Threading.Channels;

namespace API.Tools.EventQueue;

/// <summary>
/// Represents an asynchronous queue for event data, supporting enqueuing and sequential reading of events.
/// </summary>
public interface IEventQueue
{
    /// <summary>
    /// Asynchronously queues the specified event for processing.
    /// </summary>
    /// <param name="eventItem">The event data to be queued. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the queue operation.</param>
    ValueTask QueueAsync(EventData eventItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously reads all available event data as a stream.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous read operation.</param>
    /// <returns>An asynchronous stream of <see cref="EventData"/> objects representing all available events. The stream
    /// completes when no more events are available.</returns>
    IAsyncEnumerable<EventData> ReadAllAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Provides a thread-safe, asynchronous queue for event data, enabling producers to enqueue events and consumers to
/// process them asynchronously.
/// </summary>
/// <param name="channel">The channel used as the underlying transport for event data.
/// The channel should be configured for the desired concurrency and capacity requirements of the event processing scenario.</param>
public class EventQueue(Channel<EventData> channel) : IEventQueue
{
    private readonly Channel<EventData> _channel = channel;

    /// <summary>
    /// Function for asynchronously adding an item to the queue.
    /// </summary>
    /// <param name="eventItem">An data object containing the type of event and the corresponding data required to handle the event</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns></returns>
    public async ValueTask QueueAsync(EventData eventItem, CancellationToken cancellationToken = default)
    {
        try
        {
            await _channel.Writer.WriteAsync(eventItem, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            throw new Exception("Channel is currently closed");
        }
    }

    /// <summary>
    /// A function to asynchronously read the contents of the queue and return them.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>An asynchronous stream of <see cref="EventData"/> objects representing all available events. The stream
    /// completes when no more events are available.</returns>
    public IAsyncEnumerable<EventData> ReadAllAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}