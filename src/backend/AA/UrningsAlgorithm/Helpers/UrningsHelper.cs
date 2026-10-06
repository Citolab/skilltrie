/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.EntityFrameworkCore;
using Models;

namespace AA.UrningsAlgorithm.Helpers;

public class UrningsHelper(AppDbContext context)
{
    public async Task<List<MatchedUrns>> MatchedUrnsUserItem(int userId, int itemId)
    {
        Item item; ItemUrn itemUrnDb;
        try
        {
            item = await context.Items.Include(i => i.Scopes)
                .SingleAsync(iu => itemId == iu.Id);
            itemUrnDb = await context.ItemUrns.SingleAsync(iu => iu.ItemId == itemId);
        }
        catch (InvalidOperationException e)
        {
            throw new Exception("Item urn not found");
        }
        
        List<MatchedUrns> matchedUrns = new List<MatchedUrns>();

        foreach (Scope topic in item.Scopes.ToList())
        {
            if (!context.UserScopeProgress.Any(uu => userId == uu.UserId && topic.Id == uu.ScopeId))
                throw new Exception("User urn not found");

            UserScopeProgress userTopicProgress = await context.UserScopeProgress
                .SingleAsync(uu => userId == uu.UserId && topic.Id == uu.ScopeId);

            Urn playerUrn = Urn.FromObject(userTopicProgress); Urn itemUrn = Urn.FromObject(itemUrnDb);
            MatchedUrns urns = new MatchedUrns { PlayerUrn = playerUrn, ItemUrn = itemUrn };
            matchedUrns.Add(urns);
        }

        return matchedUrns;
    }
}