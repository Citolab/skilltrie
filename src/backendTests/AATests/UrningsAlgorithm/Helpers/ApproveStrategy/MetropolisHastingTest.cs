/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Reflection;
using AA.UrningsAlgorithm;
using AA.UrningsAlgorithm.Helpers;
using Xunit;

namespace AATests.UrningsAlgorithm.Helpers.ApproveStrategy;

using HarmonyLib;


[HarmonyPatch(typeof(Random), nameof(Random.NextDouble))]
public class RandomPatch
{
    public static double ReturnValue = 0.5d;
    static bool Prefix(ref double __result)
    {
        __result = ReturnValue; // force the return value
        return false;   // skip original method
    }
}

public class MetropolisHastingTest
{
    private readonly Harmony _harmony = new ("MetropolisTest");

    public void Dispose() => _harmony.UnpatchAll("MetropolisTest");

    [Fact]
    public void SurvivesMetropolisHasting_AlwaysAcceptWhenHigherDensity()
    {
        MatchedUrns oldUrns = GenerateMatchedUrns(4, 6, 10);
        MatchedUrns newUrns = GenerateMatchedUrns(5, 5, 10);
        Assert.True(new MetropolisHasting().Approve(oldUrns, newUrns));
        
        MatchedUrns oldUrns2 = GenerateMatchedUrns(10, 0, 10);
        MatchedUrns newUrns2 = GenerateMatchedUrns(9, 1, 10);
        Assert.True(new MetropolisHasting().Approve(oldUrns2, newUrns2));
    }

    [Fact]
    public void SurvivesMetropolisHasting_AcceptWhenChanceIsSet()
    {
        MatchedUrns oldUrns = GenerateMatchedUrns(8, 6, 10); // 0.32 + 0.12 = 0.44
        MatchedUrns newUrns = GenerateMatchedUrns(9, 5, 10); // 0.45 + 0.05 = 0.50
        _harmony.PatchAll();
        RandomPatch.ReturnValue = 0.87d;
        
        Assert.True(new MetropolisHasting().Approve(oldUrns, newUrns));
    }
    
    [Fact]
    public void SurvivesMetropolisHasting_RejectWhenChanceIsSet()
    {
        MatchedUrns oldUrns = GenerateMatchedUrns(8, 6, 10); // 0.32 + 0.12 = 0.44
        MatchedUrns newUrns = GenerateMatchedUrns(9, 5, 10); // 0.45 + 0.05 = 0.50
        _harmony.PatchAll();
        RandomPatch.ReturnValue = 0.89d;
        
        Assert.False(new MetropolisHasting().Approve(oldUrns, newUrns)); 
    }

    private MatchedUrns GenerateMatchedUrns(int playerGreenBalls, int itemGreenBalls, int n)
    {
        Urn playerUrn = new Urn(playerGreenBalls, n - playerGreenBalls);
        Urn itemUrn = new Urn(itemGreenBalls, n - itemGreenBalls);
        return new MatchedUrns { PlayerUrn = playerUrn, ItemUrn = itemUrn };
    }
}