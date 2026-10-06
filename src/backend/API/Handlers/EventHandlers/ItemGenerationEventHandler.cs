/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.NewItemsAlgorithm;
using API.Services;
using API.Tools.EventQueue;

namespace API.Handlers.EventHandlers;

/// <summary>
/// Concrete implementation of the handler used for item generation. 
/// </summary>
/// <param name="AIService"></param>
public class ItemGenerationEventHandler(IAIService AIService) : IEventHandler
{
    /// <summary>
    /// Indicates whether the event handler can handle the provided event
    /// </summary>
    /// <param name="eventType">The event that is being tried to handle</param>
    /// <returns>True whenever the current handler can handle the provided event</returns>
    public bool CanHandle(EventType eventType)
        => eventType == EventType.ItemGeneration;

    /// <summary>
    /// Processes the specified event data asynchronously by generating a question for the associated topic.
    /// </summary>
    /// <param name="data">The event data to process. The <see cref="EventData.Data"/> property must be of type <see
    /// cref="NewItemsRecord"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="data"/> does not contain a <see cref="NewItemsRecord"/> in its <c>Data</c> property.</exception>
    public async Task ProcessAsync(EventData data, CancellationToken cancellationToken)
    {
        if (data.Data is not NewItemsRecord record)
        {
            throw new ArgumentException($"Expected NewItemsRecord, but got {data.Data.GetType().Name}");
        }

        if (record.TotalNewItems <= 0)
            return;

        await AIService.MakeQuestions(record.TopicId, record.TotalNewItems);
    }
}