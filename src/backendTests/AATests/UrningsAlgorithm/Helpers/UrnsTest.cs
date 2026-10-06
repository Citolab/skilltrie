/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.UrningsAlgorithm.Helpers;
using Xunit;

namespace AATests.UrningsAlgorithm.Helpers;

public class RandomUrnableObject(int greenBalls, int redBalls)
{
    public int GreenBalls { get; set; } = greenBalls;
    public int RedBalls { get; set; } = redBalls;
}

public class UrnsTest
{
    [Fact]
    public void SwapBalls_ShouldSwap()
    {
        Urn urn = new Urn(7, 2);
        urn._swapBalls(2);
        Assert.Equal(9, urn.GreenBalls);
        Assert.Equal(0, urn.RedBalls);
        
        Urn urn2 = new Urn(9, 5);
        urn2._swapBalls(3);
        Assert.Equal(12, urn2.GreenBalls);
        Assert.Equal(2, urn2.RedBalls);

        Urn urn3 = new Urn(6, 4);
        urn3._swapBalls(-5);
        Assert.Equal(1, urn3.GreenBalls);
        Assert.Equal(9, urn3.RedBalls);
    }

    [Fact]
    public void SwapBalls_ShouldNotSwapWhen0()
    {
        Urn urn = new Urn(0, 10);
        urn._swapBalls(-2);
        Assert.Equal(0, urn.GreenBalls);
        Assert.Equal(10, urn.RedBalls);
        
        Urn urn2 = new Urn(12, 0);
        urn2._swapBalls(3);
        Assert.Equal(12, urn2.GreenBalls);
        Assert.Equal(0,  urn2.RedBalls);
    }

    [Fact]
    public void FromObject_CopiesPropertiesRight()
    {
        RandomUrnableObject randomUrnableObject = new RandomUrnableObject(10, 4);
        Urn urn = Urn.FromObject(randomUrnableObject);
        Assert.Equal(10, urn.GreenBalls);
        Assert.Equal(4, urn.RedBalls);
    }
    
    [Fact]
    public void ExportBalls_CopiesPropertiesRight()
    {
        RandomUrnableObject randomUrnableObject = new RandomUrnableObject(10, 4);
        Urn urn = Urn.FromObject(randomUrnableObject);
        urn._swapBalls(2);
        urn.ExportBalls();
        Assert.Equal(12, randomUrnableObject.GreenBalls);
        Assert.Equal(2, randomUrnableObject.RedBalls);
    }
}