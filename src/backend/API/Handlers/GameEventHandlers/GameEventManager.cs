/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Tools;

namespace API.Handlers.GameEventHandlers;

public interface IGameEventManager
{
    public Task TriggerGameEvent(GameEventData data);
}

/// <summary>
/// An interface indicating a class listens to the game events being triggered by the game event manager.
/// </summary>
public interface IGameEventListener
{
    public Task GameEventTriggered(GameEventData data);
}

/// <summary>
/// A wrapper class for triggering game events
/// </summary>
public class GameEventManager(IServiceProvider serviceProvider, TimeTool timeTool) : IGameEventManager
{
    public async Task TriggerGameEvent(GameEventData data)
    {
        data.FiredAt = timeTool.Now();
        await CallEvent(async listener => await listener.GameEventTriggered(data));
    }
    
    /// <summary>
    /// Utility function to create a scope of all the services registered as listeners and call them.
    /// </summary>
    /// <param name="callListener">An async method to perform an action on the listeners,
    /// this makes it flexible which event to trigger on the listeners</param>
    private async Task CallEvent(Func<IGameEventListener, Task> callListener)
    {
        using var scope = serviceProvider.CreateScope();
        IEnumerable<IGameEventListener> listeners = 
            scope.ServiceProvider.GetServices<IGameEventListener>();

        foreach (IGameEventListener listener in listeners)
            await callListener(listener);
    }
}