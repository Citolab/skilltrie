/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Threading.Channels;
using API.Services;

namespace API.Tools.EventQueue;

/// <summary>
/// Represents the data associated with an event, including its type and payload. Provides a generalized structure for the event data
/// </summary>
public class EventData(EventType eventType, object data)
{
    public EventType EventType { get; } = eventType;
    public object Data { get; } = data;
    public int Attempts { get; set; } = 0;
}

/// <summary>
/// Enum containing the events that can be handled by the queue.
/// </summary>
public enum EventType
{
    ItemGeneration,
    ItemReported
}