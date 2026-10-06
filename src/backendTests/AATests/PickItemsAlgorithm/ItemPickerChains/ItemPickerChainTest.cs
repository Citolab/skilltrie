/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.PickItemsAlgorithm.ItemPickerChains;
using AA.PickItemsAlgorithm.ItemPickers;
using Models;
using Xunit;

namespace AATests.PickItemsAlgorithm.ItemPickerChains;

/// <summary>
/// These tests are to test the functionality that is related to the BaseItemPicker. However since it is an abstract class,
/// (simple) derived classes are used to actually test this functionality and we will assume the item picking of these
/// derived classes work
/// </summary>
public class ItemPickerChainTest
{
    private static int _currentItemId = 0;
    public static Item CreateItem(Item.ItemSource source = Item.ItemSource.LLM, int id = -1)
    {
        var item = new Item
        {
            Id = id >= 0 ? id : _currentItemId++,
            Source = source,
        };

        return item;
    }
    
    [Fact]
    public void PickNItems_SelectsOnlyGeneratedWhenEnough()
    {
        List<Item.ItemSource> sources =
        [
            Item.ItemSource.LLM,
            Item.ItemSource.LLM,
            Item.ItemSource.Databank,
            Item.ItemSource.Databank,
            Item.ItemSource.LLM,
            Item.ItemSource.Imported,
            Item.ItemSource.Databank,
            Item.ItemSource.LLM,
            Item.ItemSource.LLM,
            Item.ItemSource.Imported
        ];
        List<Item> items = sources.Select(CreateItem).ToList();
        ItemPickerChain itemPickerChain = new ItemPickerChain();
        itemPickerChain.AddPicker(new AiGeneratedItemPicker());
        itemPickerChain.AddPicker(new NonGeneratedItemPicker());

        List<Item> selectedItems = itemPickerChain.PickNItems(items, 5);
        
        Assert.Equal(5, selectedItems.Count(i => i.Source == Item.ItemSource.LLM));
        Assert.Equal(0, selectedItems.Count(i => i.Source != Item.ItemSource.LLM));
    }

    [Fact]
    public void PickNItems_GoesToNextInChain()
    {
        List<Item.ItemSource> sources =
        [
            Item.ItemSource.LLM,
            Item.ItemSource.LLM,
            Item.ItemSource.Databank,
            Item.ItemSource.Databank,
            Item.ItemSource.Imported,
            Item.ItemSource.LLM,
            Item.ItemSource.Imported,
            Item.ItemSource.Databank
        ];
        List<Item> items = sources.Select(CreateItem).ToList();
        ItemPickerChain itemPickerChain = new ItemPickerChain();
        itemPickerChain.AddPicker(new AiGeneratedItemPicker());
        itemPickerChain.AddPicker(new NonGeneratedItemPicker());

        List<Item> selectedItems = itemPickerChain.PickNItems(items, 5);
        
        Assert.Equal(3, selectedItems.Count(i => i.Source == Item.ItemSource.LLM));
        Assert.Equal(2, selectedItems.Count(i => i.Source != Item.ItemSource.LLM));
    }
    
    [Fact]
    public void PickNItems_ThrowsWhenNotEnough()
    {
        List<Item.ItemSource> sources =
        [
            Item.ItemSource.LLM,
            Item.ItemSource.LLM,
            Item.ItemSource.Databank,
            Item.ItemSource.Databank,
            Item.ItemSource.Imported,
            Item.ItemSource.LLM,
            Item.ItemSource.Imported
        ];
        List<Item> items = sources.Select(CreateItem).ToList();
        ItemPickerChain itemPickerChain = new ItemPickerChain();
        itemPickerChain.AddPicker(new AiGeneratedItemPicker());

        Assert.ThrowsAny<Exception>(() => itemPickerChain.PickNItems(items, 5));
    }
    
    [Fact]
    public void PickNItems_ThrowsWhenNotEnoughChain()
    {
        List<Item.ItemSource> sources =
        [
            Item.ItemSource.LLM,
            Item.ItemSource.LLM,
            Item.ItemSource.Databank,
            Item.ItemSource.Databank,
            Item.ItemSource.Imported,
            Item.ItemSource.LLM,
            Item.ItemSource.Imported
        ];
        List<Item> items = sources.Select(CreateItem).ToList();
        ItemPickerChain itemPickerChain = new ItemPickerChain();
        itemPickerChain.AddPicker(new AiGeneratedItemPicker());
        itemPickerChain.AddPicker(new NonGeneratedItemPicker());

        Assert.ThrowsAny<Exception>(() => itemPickerChain.PickNItems(items, 10));
    }
    
    [Fact]
    public void PickNItems_RedundantChain()
    {
        List<Item.ItemSource> sources =
        [
            Item.ItemSource.LLM,
            Item.ItemSource.LLM,
            Item.ItemSource.Databank,
            Item.ItemSource.Databank,
            Item.ItemSource.LLM,
            Item.ItemSource.Imported,
            Item.ItemSource.Databank,
            Item.ItemSource.LLM,
            Item.ItemSource.LLM,
            Item.ItemSource.Imported
        ];
        List<Item> items = sources.Select(CreateItem).ToList();
        ItemPickerChain itemPickerChain = new ItemPickerChain();
        itemPickerChain.AddPicker(new AiGeneratedItemPicker());
        itemPickerChain.AddPicker(new AiGeneratedItemPicker());
        itemPickerChain.AddPicker(new NonGeneratedItemPicker());
        itemPickerChain.AddPicker(new NonGeneratedItemPicker());

        List<Item> selectedItems = itemPickerChain.PickNItems(items, 8);
        
        Assert.Equal(5, selectedItems.Count(i => i.Source == Item.ItemSource.LLM));
        Assert.Equal(3, selectedItems.Count(i => i.Source != Item.ItemSource.LLM));
    }
}