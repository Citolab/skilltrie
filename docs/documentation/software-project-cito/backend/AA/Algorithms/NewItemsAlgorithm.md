# New Items Algorithm

The New Items Algorithm is an adaptive algorithm that is used for predicting when new items for a specific topic should be generated. The implementation follows the same paradigm as the other [adaptive algorithms](../index.md). Having an interface defining the general structure and one or multiple implementations.

### INewItemsAlgorithm
The interface contains one function which returns a NewItemsRecord containing the topicId and how many items should be generated.
<br>

```csharp
public interface INewItemsAlgorithm
{
    Task<NewItemsRecord> NewItemBatch(int topicId, int userId);
}

public record NewItemsRecord
{
    public required int TopicId { get; set; }
    public required int UserId { get; set; }
    public int TotalNewItems { get; set; } = 0;
    public int OpenEnded { get; set; } = 0;
    public int MultipleChoice { get; set; } = 0;
    public int Numerical { get; set; } = 0;
}
```

### SimpleNIA
The Simple New Items Algorithm is the first and simplest version of the new items algorithm. 

The algorithm works as follows:
1. Retrieve the time before an item can be seen by a user again from the active setting
2. Get the number of active items for the specified topic
3. Get the number of items belonging to the specified topic, which are currently not available because of the cooldown time.
4. Return a NewItemsRecord indicating how many items should be generated.
    - When new items should be generated, all items that need to be generated are set to multiple choice questions, and TotalNewItems is set the number of items.
    - When no items should be generated, TotalNewItems is set to 0.