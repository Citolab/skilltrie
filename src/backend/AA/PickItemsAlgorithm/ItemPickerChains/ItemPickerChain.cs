/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.PickItemsAlgorithm.ItemPickers;
using Models;

namespace AA.PickItemsAlgorithm.ItemPickerChains;

public class ItemPickerChain
{
    private readonly LinkedList<IItemPicker> _chain = new();
    private readonly List<IItemPicker> _traversed = new();

    public ItemPickerChain()
    {
        _chain.AddFirst(new LinkedListNode<IItemPicker>(null));
    }

    public void AddPicker(IItemPicker next)
    {
        _chain.AddLast(next);
    }

    public List<Item> PickNItems(ICollection<Item> availableItems, int itemAmount)
    {
        List<Item> items = new List<Item>();
        LinkedListNode<IItemPicker>? currentItemPicker = _chain.First;
        while (items.Count < itemAmount)
        {
            int itemAmountLeft = itemAmount - items.Count;
            if (currentItemPicker.Next == null) throw BuildException();
            currentItemPicker = currentItemPicker.Next;
            _traversed.Add(currentItemPicker.Value);
            
            List<Item> selectedItems = currentItemPicker.Value.PickItems(availableItems);
            List<Item> nSelectedItems = selectedItems
                .Where(i => i.Active)
                .Take(itemAmountLeft)
                .ToList();
            
            items = items.Concat(nSelectedItems).ToList();
            availableItems = availableItems.Except(nSelectedItems).ToList();
        }

        return items;
    }

    private Exception BuildException()
    {
        string error = "";
        foreach (var responsibility in _traversed) error += "from " + responsibility.GetType().Name + "\n";
        error += "from " + GetType().Name + "\n";
        return new Exception(error + "Not enough questions in available items");
    }
}