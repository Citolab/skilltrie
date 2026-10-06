# IItemPicker

Let's first begin with the concept of item picking. It can be best described by a pure funtion which takes a list of items, filters the items which are suitable according to the item picker and ranking them by suitability if such ranking exists. Therefore we have made the following interface

```cs
public interface IItemPicker
{
    public List<Item> PickItems(ICollection<Item> items);
}
```
<br/>

# BaseItemPicker

There are different kinds of item pickers which should be open for combination. So there is an item picker which picks AI generated items and there is an item picker which picks not generated items. There is also an item picker which picks items with fair matchmaking and one which does that with random selection. <br/>
We need to be able to combine these two properties. So we need to be able to pick AI items with fair matchmaking, AI items with random selection, not AI items with fair matchmaking and not AI items with random selection. <br/>
To facilitate this combination, we decided to use a decorator pattern. That is exactly what the BaseItemPicker is for.

```cs
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
```

# Concrete Item Pickers
The concrete item pickers are doing the real work. They implement the PickItems method to pick suitable items and then call the next item picker to again pick suitable items according to the decorator pattern. An example: 
```cs
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
```