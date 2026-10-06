/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Reflection;
using AA.PickItemsAlgorithm;
using AATests.PickItemsAlgorithm.ItemPickerChains;
using FluentAssertions;
using Models;
using Xunit;

namespace AATests.PickItemsAlgorithm;

public class V1PickItemsTest
{
    private readonly V1PickItems _algorithm = new(SqliteInMemoryContextFactory.Create());
    
    [Fact]
    public void PickItems_SelectsRightDistribution()
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
            Item.ItemSource.Databank,
            Item.ItemSource.Databank,
            Item.ItemSource.Imported,
            Item.ItemSource.Databank,
        ];
        List<Item> items = sources.Select(ItemPickerChainTest.CreateItem).ToList();
        
        List<Item> selectedItems = _algorithm.PickItems(items);
        
        Assert.Equal(8, selectedItems.Count(i => i.Source != Item.ItemSource.LLM));
        Assert.Equal(2, selectedItems.Count(i => i.Source == Item.ItemSource.LLM));
    }
    
    [Fact]
    public void PickItems_NoDuplicate()
    {
        List<Item.ItemSource> sources =
        [
            Item.ItemSource.LLM,
            Item.ItemSource.Databank,
            Item.ItemSource.Databank,
            Item.ItemSource.Imported,
            Item.ItemSource.Imported,
            Item.ItemSource.Databank,
            Item.ItemSource.Databank,
            Item.ItemSource.Imported,
            Item.ItemSource.Databank,
            Item.ItemSource.Imported,
        ];
        List<Item> items = sources.Select(ItemPickerChainTest.CreateItem).ToList();

        List<Item> selectedItems = _algorithm.PickItems(items);
        
        Assert.Equal(9, selectedItems.Count(i => i.Source != Item.ItemSource.LLM));
        Assert.Equal(1, selectedItems.Count(i => i.Source == Item.ItemSource.LLM));
        selectedItems.Should().BeEquivalentTo(selectedItems.Distinct().ToList());
    }
}