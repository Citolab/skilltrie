/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace AA.UrningsAlgorithm.Helpers;

public interface ApproveStrategy
{
    public bool Approve(MatchedUrns oldUrns, MatchedUrns newUrns);
}

public enum ApproveStrategyEnum
{
    AlwaysApprove,
    MetropolisHasting,
}

public static class ApproveStrategyMap
{
    public static ApproveStrategy Map(this ApproveStrategyEnum choice)
    {
        switch (choice)
        {
            case ApproveStrategyEnum.AlwaysApprove: return new AlwaysApprove();
            case ApproveStrategyEnum.MetropolisHasting: return new MetropolisHasting();
            default: return new AlwaysApprove();
        }
    }
}

public class AlwaysApprove : ApproveStrategy
{
    public bool Approve(MatchedUrns oldUrns, MatchedUrns newUrns)
    {
        return true;
    }
}

public class MetropolisHasting : ApproveStrategy
{
    /// <summary>
    /// Determines with <i>random.NextDouble()</i> whether it survives the MetropolisHasting step.
    /// </summary>
    /// <returns>Whether the step survives the Metropolis Hasting step</returns>
    public bool Approve(MatchedUrns oldUrns, MatchedUrns newUrns)
    {
        double oldDensity = MetropolisHastingDensity(oldUrns);
        double newDensity = MetropolisHastingDensity(newUrns);
        Random random = new();
        return random.NextDouble() <= newDensity / oldDensity;
    }
    
    /// <summary>
    /// The f(x) in the Metropolis Hasting step. Used to make the limiting distribution of an item independent of that of the opponent. 
    /// </summary>
    /// <returns>The metrpolis hasting density</returns>
    private double MetropolisHastingDensity(MatchedUrns matchedUrns)
    {
        Urn playerUrn = matchedUrns.PlayerUrn; Urn itemUrn = matchedUrns.ItemUrn;
        int n_p = playerUrn.GreenBalls + playerUrn.RedBalls; int n_q = itemUrn.GreenBalls + itemUrn.RedBalls;
        double u_p = (double)playerUrn.GreenBalls / n_p; double u_q = (double)itemUrn.GreenBalls / n_q;
        
        double pHit = u_p * (1 - u_q) + u_q * (1 - u_p); 
        if (pHit == 0)
            return Double.MaxValue;

        return 1d / pHit;
    }
}