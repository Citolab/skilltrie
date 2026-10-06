# Models

We use the **Entity Framework Core (EF Core)**, where a **Model** represents the data structures of our Web Application.

## What is a Model?

A **Model** is a class that defines the shape of our data. It typically includes:

-   **Properties** that map to database columns.
-   **Relationships** to other models.
-   **Validation rules** to ensure data integrity.

In EF Core, models are often referred to as **entity classes**, as they represent entities that are stored in the database.

### Example

```csharp
public class Topic
{
    public int Id { get; set; }                 // Primary key

    [Required]
    public string Name { get; set; } = null!;   // Name property

    // Navigation properties
    [IgnoreDataMember]
    public ICollection<TopicClosure> Ancestors { get; set; } = [];
    [IgnoreDataMember]
    public ICollection<TopicClosure> Descendants { get; set; } = [];
    [IgnoreDataMember]
    public ICollection<TopicItem> TopicItems { get; set; } = [];
    [IgnoreDataMember]
    public ICollection<Item> Items { get; } = [];
}
```
