/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using Xunit;
using FluentAssertions;
using AA.PickItemsAlgorithm.ItemPickers;
using AATests.PickItemsAlgorithm.ItemPickerChains;

namespace AATests.PickItemsAlgorithm.ItemPickers;

public class AiGeneratedItemPickerTests
{
    [Fact]
    public void PickItems_SelectsOnlyAIQuestions()
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
        List<Item> items = sources.Select(ItemPickerChainTest.CreateItem).ToList();
        
        List<Item> selectedItems = new AiGeneratedItemPicker().PickItems(items);
        
        selectedItems.Select(item => item.Id).Should().BeEquivalentTo(
            new List<int>{0, 1, 5}
        );
    }
}

public class NonGeneratedItemPickerTests
{
    [Fact]
    public void PickItems_SelectsOnlyAIQuestions()
    {
        List<Item.ItemSource> sources = new List<Item.ItemSource>
        {
            Item.ItemSource.Databank, 
            Item.ItemSource.LLM, 
            Item.ItemSource.Imported,
            Item.ItemSource.Databank,
            Item.ItemSource.Imported,
            Item.ItemSource.LLM,
            Item.ItemSource.Imported,
            Item.ItemSource.Databank,
            Item.ItemSource.LLM,
            Item.ItemSource.LLM,
            Item.ItemSource.LLM,
            Item.ItemSource.Imported
        };

        List<Item> items = sources.Select(ItemPickerChainTest.CreateItem).ToList();
        List<Item> selectedItems = new NonGeneratedItemPicker().PickItems(items);
        
        selectedItems.Select(item => item.Id).Should().BeEquivalentTo(
            new List<int>{0, 2, 3, 4, 6, 7, 11}
        );
    }
}