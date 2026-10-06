/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace AA.ItemDisableAlgorithm;

/// <summary>
/// An item validity algorithm which validates an item if it has less than <see cref="_reportCountThreshold"/> reports
/// have been made on the item.
/// </summary>
[Algorithm("Simple Item Validity")]
public class SimpleItemValidity(AppDbContext context) : IItemValidityAlgorithm
{
    private readonly int _reportCountThreshold = 5;
    
    public async Task<bool> AssesItemValidity(int itemId)
    {
        Item item = await context.Items.FindAsync(itemId) ?? throw new Exception("requested item not found");
        int reportCount = context.Reports.Count(r => r.ItemId == itemId);
        return reportCount < _reportCountThreshold;
    }
}