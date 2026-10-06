/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.UrningsAlgorithm.Helpers;
using Microsoft.EntityFrameworkCore;
using Models;

namespace AA.UrningsAlgorithm;

[Algorithm("Urnings Algorithm With Metropolis Hasting Step")]
public class UrningsWithMetropolis(AppDbContext context): IUrningsAlgorithm
{
    /// <summary>
    /// Query the player and item urn and update their amount of balls.
    /// Finally pushes these changes to the database.
    /// </summary>
    /// <param name="userId">The user whose urn to get</param>
    /// <param name="itemId">The item whose urn to get</param>
    /// <param name="actuallyGreen">The actual result of the latest question</param>
    /// <exception cref="Exception">Exception to throw when any of the urns does not exist.</exception>
    /// <remarks>Runs this for each domain for a given user. This means that items with multiple domains are changed
    /// multiple times for the same user, one time for each of their domains.</remarks>
    public async Task UpdateUrnings(int userId, int itemId, bool actuallyGreen)
    {
        List<MatchedUrns> matchedUrnsList = await new UrningsHelper(context).MatchedUrnsUserItem(userId, itemId);
        foreach (MatchedUrns matchedUrns in matchedUrnsList)
        {
            bool exchanged = matchedUrns.UrningsAlgorithm(actuallyGreen, approveStrategyEnum: ApproveStrategyEnum.MetropolisHasting);
            if (!exchanged) return;
            
            matchedUrns.ExportBalls();
        }
        await context.SaveChangesAsync(); 
    }
}