/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Reflection;

namespace AA;

/// <summary>
/// Interface, mainly used for testing purposes. 
/// </summary>
/// <typeparam name="T">The interface of the algorithm to make a registry of</typeparam>
public interface IAlgorithmRegistry<T>
{
    /// <summary>
    /// A method to extract an algorithm instance from the key attribute. 
    /// </summary>
    /// <param name="key">The name / key attribute of the algorithm</param>
    /// <returns>An instance of the algorithm, implementing the T interface.</returns>
    T Get(String key);
}

/// <summary>
/// Default implementation of the algorithm registry.
/// </summary>
/// <typeparam name="T">The interface of the algorithm to make a registry of</typeparam>
public class AlgorithmRegistry<T> : IAlgorithmRegistry<T>
{
    /// <summary>
    /// A registry that caches the algorithms, as reflection is expensive. 
    /// </summary>
    private readonly IReadOnlyDictionary<string, T> _registry;

    /// <summary>
    /// Instantiator that extracts all algorithms implementing the T interface and places it into the registry. 
    /// Called by the DI. 
    /// </summary>
    /// <param name="algorithms">The algorithms, as found by the Attribute functionality in dotnet</param>
    public AlgorithmRegistry(IEnumerable<T> algorithms)
    {
        _registry = algorithms
            .Where(a => a != null && a.GetType().GetCustomAttribute<Algorithm>() != null) // Filter out null algorithms
            .ToDictionary(a => a!.GetType().GetCustomAttribute<Algorithm>()!.Key); // Bangs are safe, as non-null is asserted
    }
    
    /// <summary>
    /// Get functionality, called using registry.Get("Name of algorithm") to instantly get an instance of the desired algorithm.
    /// </summary>
    /// <param name="key">The name used to get an algorithm.</param>
    /// <returns>An instance of the algorithm</returns>
    public T Get(String key) => _registry.TryGetValue(key, out var algorithm) 
        ? algorithm : throw new KeyNotFoundException("Algorithm not found");
}
