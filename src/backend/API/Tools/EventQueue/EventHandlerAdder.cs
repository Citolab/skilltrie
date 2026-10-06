/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Reflection;
using API.Handlers.EventHandlers;

namespace API.Tools.EventQueue;

/// <summary>
/// A class which will add all the classes which implement <see cref="IEventHandler"/>
/// of the given assembly to the collection of services.
/// </summary>
public static class EventHandlerAdder
{
    public static IServiceCollection AddEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        var eventHandlers = assembly
            .GetTypes()
            .Where(t => t.IsClass
                        && !t.IsInterface
                        && !t.IsAbstract 
                        && t.GetInterfaces().Contains(typeof(IEventHandler)));

        foreach (var type in eventHandlers)
            services.AddScoped(typeof(IEventHandler), type);

        return services;
    }
}