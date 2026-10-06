/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace AA.UrningsAlgorithm.Helpers;

public class Urn
{
    public int GreenBalls { get; private set; }
    public int RedBalls { get; private set; }
    
    public dynamic? OriginalUrn { get; init; }

    private Urn() {}

    public Urn(int greenBalls, int redBalls)
    {
        this.GreenBalls = greenBalls;
        this.RedBalls = redBalls;
    }

    /// <summary>
    /// Creates an Urn object from an object which has properties called GreenBalls and RedBalls
    /// </summary>
    public static Urn FromObject(dynamic urn)
    {
        return new Urn { GreenBalls = urn.GreenBalls, RedBalls = urn.RedBalls, OriginalUrn = urn};
    }

    /// <summary>
    /// Helper function to swap the balls between the Green and Red balls.
    /// Doesn't change balls if any is at a value of zero. 
    /// </summary>
    /// <param name="greenBallsToAdd">The amount of green balls to add (can be negative) </param>
    public void _swapBalls(int greenBallsToAdd)
    {
        if (greenBallsToAdd > 0 && RedBalls <= 0) return;
        if (greenBallsToAdd < 0 && GreenBalls <= 0) return;

        GreenBalls += greenBallsToAdd;
        RedBalls -= greenBallsToAdd;
    }

    /// <summary>
    /// Writes the green balls and red balls properties to an external urn
    /// </summary>
    /// <param name="externalUrn">The external urn to write the properties to.
    /// Must have <i>GreenBalls</i> and <i>RedBalls</i> properties</param>
    public void ExportBallsTo(dynamic externalUrn)
    {
        externalUrn.GreenBalls = this.GreenBalls;
        externalUrn.RedBalls = this.RedBalls;
    }

    /// <summary>
    /// Exports the balls to the urn object this urn was created. See <see cref="ExportBallsTo"/>
    /// </summary>
    public void ExportBalls()
    {
        ExportBallsTo(OriginalUrn);
    }
}

public class MatchedUrns
{
    public required Urn PlayerUrn { get; init; }
    public required Urn ItemUrn { get; init; }

    private static MatchedUrns Copy(MatchedUrns urns)
    {
        Urn newPlayerUrn = Urn.FromObject(urns.PlayerUrn);
        Urn newItemUrn = Urn.FromObject(urns.ItemUrn);
        return new MatchedUrns { PlayerUrn = newPlayerUrn, ItemUrn = newItemUrn };
    }

    /// <summary>
    /// The actual Urnings algorithm flow.
    /// </summary>
    /// <param name="actuallyGreen">Whether the user responded correctly in reality.</param>
    /// <param name="approveStrategyEnum"></param>
    /// <returns></returns>
    public bool UrningsAlgorithm(bool actuallyGreen, ApproveStrategyEnum approveStrategyEnum = ApproveStrategyEnum.AlwaysApprove)
    {
        ApproveStrategy approveStrategy = approveStrategyEnum.Map();
        bool? userPredictionGreen = _predictMatchOutcome();
        if (userPredictionGreen is null) return false; // No hit, so stop algorithm.

        // +1 if unpredicted correct answer, -1 if unpredicted incorrect answer. 0 if prediction matches reality. 
        int playerGreenBallsToAdd = (actuallyGreen ? 1 : 0) - (userPredictionGreen.Value ? 1 : 0);
        MatchedUrns newUrns = CopyAndExchangeBalls(playerGreenBallsToAdd);

        bool commit = approveStrategy.Approve(this, newUrns);
        if (commit) ExchangeBalls(playerGreenBallsToAdd);
        return commit;
    }

    private MatchedUrns CopyAndExchangeBalls(int playerGreenBallsToAdd)
    {
        MatchedUrns newUrns = Copy(this);
        newUrns.ExchangeBalls(playerGreenBallsToAdd);
        return newUrns;
    }
    
    /// <summary>
    /// A method that gives a prediction (true or false) of wether we predict the user gets the answer right based on urns. 
    /// </summary>
    /// <returns>A boolean to show whether the player is predicted to get the question right (green)</returns>
    private bool? _predictMatchOutcome()
    {
        int userTotal = PlayerUrn.GreenBalls + PlayerUrn.RedBalls; 
        int itemTotal = ItemUrn.GreenBalls + ItemUrn.RedBalls;

        Random random = new();

        double probability_user_green = PlayerUrn.GreenBalls / userTotal * (1 - ItemUrn.GreenBalls / itemTotal); 
        double probability_item_green = ItemUrn.GreenBalls / itemTotal * (1 - PlayerUrn.GreenBalls / userTotal);
        double probability_user_predict_green = probability_user_green / (probability_user_green + probability_item_green);

        return random.NextDouble() < probability_user_predict_green;         
    }
    
    /// <summary>
    /// Adds <see cref="playerGreenBallsToAdd"/> balls to the player ball from the item urn.
    /// If <see cref="playerGreenBallsToAdd"/> is positive, green balls get added to te player urn
    /// and red balls get added to the item urn, otherwise red balls get added to the player urn and
    /// green balls get added to the item urn.
    /// Of course the equal amount of opposite balls is subtracted in both urns.
    /// </summary>
    private void ExchangeBalls(int playerGreenBallsToAdd)
    {
        PlayerUrn._swapBalls(playerGreenBallsToAdd);
        ItemUrn._swapBalls(-playerGreenBallsToAdd);
    }
    
    /// <summary>
    /// see <see cref="Urn.ExportBalls"/>
    /// </summary>
    public void ExportBalls()
    {
        this.PlayerUrn.ExportBalls();
        this.ItemUrn.ExportBalls();
    }
}