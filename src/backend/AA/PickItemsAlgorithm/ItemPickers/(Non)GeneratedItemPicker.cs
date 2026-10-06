/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace AA.PickItemsAlgorithm.ItemPickers;

/// <summary>
/// picks items that are not AI Generated, one of the concrete item pickers of the decorator pattern
/// </summary>
public class NonGeneratedItemPicker : BaseItemPicker
{
    public override List<Item> PickItems(ICollection<Item> availableItems)
    {
        List<Item> pickedItems = availableItems.Where(i => i.Source != Item.ItemSource.LLM).ToList();
        return base.PickItems(pickedItems);
    }
}

/// <summary>
/// picks items that are AI Generated, one of the concrete item pickers of the decorator pattern
/// </summary>
public class AiGeneratedItemPicker : BaseItemPicker
{
    public override List<Item> PickItems(ICollection<Item> availableItems)
    {
        List<Item> pickedItems = availableItems.Where(i => i.Source == Item.ItemSource.LLM).ToList();
        return base.PickItems(pickedItems);
    }
}