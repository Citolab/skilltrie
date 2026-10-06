/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA;
using Xunit;

namespace AATests;

public interface ITestAlgorithm { }

[Algorithm("algo-one")]
public class AlgorithmOne : ITestAlgorithm { }

[Algorithm("algo-two")]
public class AlgorithmTwo : ITestAlgorithm { }

public class AlgorithmWithoutAttribute : ITestAlgorithm { }

public class AlgorithmFactoryTests
{
    [Fact]
    public void Get_WithValidKey_ReturnsCorrectAlgorithm()
    {
        var factory = new AlgorithmRegistry<ITestAlgorithm>(new List<ITestAlgorithm>
        {
            new AlgorithmOne(),
            new AlgorithmTwo()
        });

        var result = factory.Get("algo-one");

        Assert.IsType<AlgorithmOne>(result);
    }

    [Fact]
    public void Get_WithInvalidKey_ThrowsKeyNotFoundException()
    {
        var factory = new AlgorithmRegistry<ITestAlgorithm>(new List<ITestAlgorithm>
        {
            new AlgorithmOne()
        });

        Assert.Throws<KeyNotFoundException>(() => factory.Get("nonexistent"));
    }

    [Fact]
    public void Get_AlgorithmWithoutAttribute_IsNotRegistered()
    {
        var factory = new AlgorithmRegistry<ITestAlgorithm>(new List<ITestAlgorithm>
        {
            new AlgorithmOne(),
            new AlgorithmWithoutAttribute()
        });

        // AlgorithmWithoutAttribute should be silently ignored
        Assert.Throws<KeyNotFoundException>(() => factory.Get("AlgorithmWithoutAttribute"));
    }

    [Fact]
    public void Get_MultipleAlgorithms_EachResolvesCorrectly()
    {
        var factory = new AlgorithmRegistry<ITestAlgorithm>(new List<ITestAlgorithm>
        {
            new AlgorithmOne(),
            new AlgorithmTwo()
        });

        Assert.IsType<AlgorithmOne>(factory.Get("algo-one"));
        Assert.IsType<AlgorithmTwo>(factory.Get("algo-two"));
    }

    [Fact]
    public void Get_EmptyAlgorithmList_ThrowsKeyNotFoundException()
    {
        var factory = new AlgorithmRegistry<ITestAlgorithm>(new List<ITestAlgorithm>());

        Assert.Throws<KeyNotFoundException>(() => factory.Get("algo-one"));
    }
}