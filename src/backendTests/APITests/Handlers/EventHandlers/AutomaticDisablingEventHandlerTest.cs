/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA;
using AA.ItemDisableAlgorithm;
using API.Handlers.EventHandlers;
using API.Tools.EventQueue;
using Microsoft.EntityFrameworkCore;
using Models;
using Moq;

namespace APITests.Handlers.EventHandlers;

public class AutomaticDisablingEventHandlerTest
{
    private readonly AppDbContext _db;
    private readonly AutomaticDisablingEventHandler _eventHandler;
    private readonly Mock<IItemValidityAlgorithm> _itemValidityAlgorithm;

    public AutomaticDisablingEventHandlerTest()
    {
        _db = SqliteInMemoryContextFactory.Create();
        var itemDisableRegistry = new Mock<IAlgorithmRegistry<IItemValidityAlgorithm>>();
        _itemValidityAlgorithm = new Mock<IItemValidityAlgorithm>();
        itemDisableRegistry.Setup(f => f.Get(It.IsAny<string>())).Returns(_itemValidityAlgorithm.Object);
        _eventHandler = new AutomaticDisablingEventHandler(_db, itemDisableRegistry.Object);
    }

    [Fact]
    public async Task ProcessAsync_ItemDeactivatesWhenItemShouldBeDisabled()
    {
        int itemId = 4;
        AddItem(itemId);
        AssesValidityReturns(true);

        await _eventHandler.ProcessAsync(data: new EventData(
                EventType.ItemReported, 
                new Dictionary<string, int> { { "itemId", itemId } }), 
            CancellationToken.None
        );
        
        Assert.True(
            (await _db.Items.SingleAsync(i => i.Id == itemId)).Active
        );
    }
    
    [Fact]
    public async Task ProcessAsync_ItemDoesNotDeactivateWhenItemShouldNotBeDisabled()
    {
        int itemId = 4;
        AddItem(itemId);
        AssesValidityReturns(false);

        await _eventHandler.ProcessAsync(data: new EventData(
                EventType.ItemReported, 
                new Dictionary<string, int> { { "itemId", itemId } }), 
            CancellationToken.None
        );
        
        Assert.False(
            (await _db.Items.SingleAsync(i => i.Id == itemId)).Active
        );
    }

    private void AssesValidityReturns(bool value)
    {
        _itemValidityAlgorithm.Setup(a => a.AssesItemValidity(It.IsAny<int>())).ReturnsAsync(value);
    }
    
    private void AddItem(int itemId)
    {
        _db.Items.Add(new Item
        {
            Id = itemId,
            QuestionText = "q",
            ResponseType = "r",
        });

        _db.SaveChangesAsync();
    }
}