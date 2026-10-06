/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA;
using AA.ItemDisableAlgorithm;
using API.Tools.EventQueue;
using Microsoft.EntityFrameworkCore;
using Models;

namespace API.Handlers.EventHandlers;

/// <summary>
/// Event Handler which handles the ItemReported event. Will use an ItemValidityAlgorithm to assess whether the item
/// is still valid, if not, it will disable it in the database.
/// </summary>
public class AutomaticDisablingEventHandler(AppDbContext context, IAlgorithmRegistry<IItemValidityAlgorithm> itemDisableRegistry) : IEventHandler
{
    public bool CanHandle(EventType eventType)
    {
        return eventType == EventType.ItemReported;
    }

    public async Task ProcessAsync(EventData data, CancellationToken cancellationToken)
    {
        IItemValidityAlgorithm itemValidityAlgorithm = itemDisableRegistry.Get("Simple Item Validity");
        if (!((Dictionary<string, int>) data.Data).TryGetValue("itemId", out int itemId)) 
            throw new Exception("Id of reported item not found");
        bool itemIsValid = await itemValidityAlgorithm.AssesItemValidity(itemId);
        if (!itemIsValid) await DisableItem(itemId);
    }

    private async Task DisableItem(int itemId)
    {
        Item item = await context.Items.SingleAsync(i => i.Id == itemId);
        item.Active = false;
        await context.SaveChangesAsync();
    }
}
