/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AATests;

public class AlgorithmServiceAdderTests
{
    [Fact]
    public void AddAlgorithms_RegistersAllImplementations()
    {
        var services = new ServiceCollection();
        var assembly = typeof(AlgorithmOne).Assembly; // ← test project assembly

        services.AddAlgorithms(assembly);

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetServices<ITestAlgorithm>().ToList();

        Assert.Contains(resolved, r => r is AlgorithmOne);
        Assert.Contains(resolved, r => r is AlgorithmTwo);
    }

    [Fact]
    public void AddAlgorithms_DoesNotRegisterInterfaces()
    {
        var services = new ServiceCollection();
        var assembly = typeof(AlgorithmOne).Assembly;

        services.AddAlgorithms(assembly);

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetServices<ITestAlgorithm>().ToList();

        Assert.All(resolved, r => Assert.False(r.GetType().IsInterface));
    }

    [Fact]
    public void AddAlgorithms_ReturnsServiceCollection_ForChaining()
    {
        var services = new ServiceCollection();
        var assembly = typeof(AlgorithmOne).Assembly;

        var result = services.AddAlgorithms(assembly);

        Assert.Same(services, result);
    }
}