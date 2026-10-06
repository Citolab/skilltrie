/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers.DTOs;
using Microsoft.EntityFrameworkCore;
using Models;
using API.Tools.Badges.ABTesting;

namespace API.Services;

public interface IBadgeService
{
    Task<List<UserBadgeDTO>> GetUserBadges(int userId);
}

public class BadgeService(AppDbContext context, IAbTestingService abTestingService) : IBadgeService
{
    /// <remarks>
    /// If no corresponding progress is in the table, the progress prop will be null in the <see cref="UserBadgeDTO"/> 
    /// </remarks>
    /// <param name="userId">The id of the user you want to get the badges from</param>
    /// <returns>A list of badges left joined with the corresponding progress based on the user id.</returns>
    public async Task<List<UserBadgeDTO>> GetUserBadges(int userId)
    {
        List<UserBadgeFlatResult> results =
            context.Database.SqlQueryRaw<UserBadgeFlatResult>(
            """
            SELECT B."Name" AS Name, "Identifier" AS Identifier, "Description" AS Description, "ProgressNeeded" AS ProgressNeeded, 
                   "Category" AS Category, "Stamp" AS StampImage, "Progress" AS Progress, 
                   "DateAccomplished" AS DateAccomplished, "X" AS X, "Y" AS Y, "Page" AS Page,
                   "FlagKey" AS FlagKey, "FlagVariant" AS FlagVariant
            FROM "Badge" B
            LEFT OUTER JOIN "BadgeProgress" BP on B."Identifier" = BP."BadgeIdentifier" and BP."UserId" = {0}
            LEFT OUTER JOIN "BadgeStamp" BST on BP."StampId" = BST."Id"
            JOIN "BadgeState" BS on B."Identifier" = BS."BadgeIdentifier"
            WHERE BS."Phase" = 2 and not BS."Excluded" and
            (BS."OpenFrom" is NULL or NOW() > BS."OpenFrom") and
            (BS."OpenUntil" is NULL or NOW() < BS."OpenUntil")
            """, userId).ToList();


        List<UserBadgeFlatResult> ABFilteredResults = await results.FilterABTesting(r =>
            new BadgeState { FlagKey = r.FlagKey, FlagVariant = r.FlagVariant },
            userId,
            abTestingService
        );

        return UserBadgeDTO.FromFlatBadge(ABFilteredResults);
    }
}