# Services
Services are another addition to the classic MVC paradigm that we have added to our backend. The Idea of a service is that most actual business logic is handled here. It is called by the Controller when it has unwrapped the incoming data from Dto's, and then processes the request. It makes use of [handlers](/backend/Handlers/index.md) for pure operations without any side effects. *(Think back to the fun times of Functional Programming.)*

Make absolutely certain that both the service and its equivalent Interface are declared for dependancy injection in `Program.cs`
## Structure and Standards
For each [Controller](/backend/Controllers/index.md) class there should be an equivalent **Service** class, and for every method inside every [Controller](/backend/Controllers/index.md) class, there should be an equivalent method in the **Service** class that is to be called from the [Controller](/backend/Controllers/index.md) method.

Each Service must also have an Interface that describes its methods. This is important for unit testing.

As much as reasonably possible, no Dto's should be used in the services. These should be unpacked in the [Controller](/backend/Controllers/index.md), and its data sent to the service. In addition to this, all *pure* functional code should be split out into a [handler](/backend/Handlers/index.md).
All Database logic ***must*** be done within the service, and not within the [controller](/backend/Controllers/index.md), or heaven forbid the [handler](/backend/Handlers/index.md).

## Example

```csharp
using backend.Handlers;
using backend.Models;

namespace backend.Services;

public interface IReportService
{
    Task CreateReport(int itemId, ItemError itemError);
}
public class ReportService(
    AppDbContext context,
    IItemService itemService,
    ReportHandler reportHandler
    ) : IReportService
{
    /// <summary>
    /// Create a new Report entry
    /// </summary>
    /// <param name="itemId">The Id of the <see cref="Item"/> that the report is about</param>
    /// <param name="itemError">The error concerning to this particular report</param>
    public async Task CreateReport(int itemId, ItemError itemError)
    {
        await itemService.VerifyItemExists(itemId);
        var newReport = reportHandler.CreateReportForDb(itemId, itemError);

        context.Reports.Add(newReport);
        await context.SaveChangesAsync();
    }
}
```