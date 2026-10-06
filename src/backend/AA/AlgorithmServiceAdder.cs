/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AA;

public static class AlgorithmServiceAdder
{
    public static IServiceCollection AddAlgorithms(this IServiceCollection services, Assembly assembly)
    {
        var implementations = assembly
            .GetTypes()
            .Where(t => t.IsClass 
                        && !t.IsInterface
                        && !t.IsAbstract
                        && t.Namespace != "AA"); // Exclude root namespace

        foreach (var type in implementations)
        {
            foreach (var iface in type.GetInterfaces()) // usually returns only one interface, but looping is more future proof. 
            {
                services.AddScoped(iface, type);
            }
        }

        return services;
    }
}