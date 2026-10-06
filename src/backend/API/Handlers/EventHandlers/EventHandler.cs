/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Services;
using API.Tools.EventQueue;

namespace API.Handlers.EventHandlers;

/// <summary>
/// Defines the base functionality that a concrete event handler should implement. 
/// These event handlers are used inside the <see cref="EventQueueService"/>.
/// </summary>
public interface IEventHandler
{
    /// <summary>
    /// Indicates whether the event handler can handle the provided event
    /// </summary>
    /// <param name="eventType">The event that is being tried to handle</param>
    /// <returns>True whenever the current handler can handle the provided event</returns>
    bool CanHandle(EventType eventType);

    /// <summary>
    /// Asynchronously processes the specified event data.
    /// </summary>
    /// <param name="data">The event data to be processed. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    Task ProcessAsync(EventData data, CancellationToken cancellationToken);
}