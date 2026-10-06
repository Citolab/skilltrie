/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace AA.PickItemsAlgorithm.ItemPickers;

public interface IItemPicker
{
    public List<Item> PickItems(ICollection<Item> items);
}

public abstract class BaseItemPicker : IItemPicker
{
    private IItemPicker? _next;

    public void SetNext(IItemPicker next)
    {
        this._next = next;
    }

    public virtual List<Item> PickItems(ICollection<Item> availableItems)
    {
        if (_next == null) return availableItems.ToList();
        return _next.PickItems(availableItems);
    }
}