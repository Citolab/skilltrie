/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers;
using API.Services;
using Models;

namespace API.Tools.Badges.ABTesting;

public static class ABBadgeFilter
{
    /// <summary>
    /// Helper method to perform AB testing on a list of badge-ish objects.
    /// For each badge checks whether it has both a flag key and variant set.
    /// If any is null, the badge survives the filter. (If the flag key or variant is not set, we assume the badge is meant for everyone)
    /// If both set, it will check the variant of the user in Posthog of the given feature flag.
    /// If that variant is equal to the variant set on the badge, the badge will survive the filter, otherwise not. 
    /// </summary>
    /// <param name="stateContainers">The badge-ish objects, which have some access to a belonging <see cref="BadgeState"/></param>
    /// <param name="getBadgeState">The function to get the badge state from a badge-ish object.</param>
    /// <param name="userId">The user for which to perform AB-testing on. Is needed to get the flag variant of the user.</param>
    /// <param name="service">The AB-Testing service. Needs to be passed as this is a static helper method</param>
    /// <typeparam name="T">The type of the badge-ish objects.</typeparam>
    /// <returns>The original list of badge-ish objects with some filtered out as described in the summary</returns>
    public static async Task<List<T>> FilterABTesting<T>(
        this List<T> stateContainers, 
        Func<T, BadgeState> getBadgeState, 
        int userId,
        IAbTestingService service
    )
    {
        IEnumerable<T> filteredStateContainers = await stateContainers
            .WhereAsync(async stateContainer =>
                {
                    BadgeState state = getBadgeState(stateContainer);
                    string? key = state.FlagKey;
                    string? variant = state.FlagVariant;
                    if (key == null || variant == null) return true;
                    
                    return AbTest.Run<bool?>(
                        await service.GetFlagAsync(userId, key), 
                        flag => (flag.Variant == null || flag.Variant == variant) // always include if variant does not exist in the feature flag
                    ) ?? true; // Always include if flag doesn't exist.
                }
            );

        return filteredStateContainers.ToList();
    }
    
    /// <summary>
    /// .Where from LINQ, but then async.
    /// </summary>
    private static async Task<IEnumerable<T>> WhereAsync<T>(this IEnumerable<T> source, Func<T, Task<bool>> predicate)
    {
        var results = await Task.WhenAll(source.Select(async x => (x, await predicate(x))));
        return results.Where(x => x.Item2).Select(x => x.Item1);
    }
}