# Controllers

All controller classes are derived from the **AspNetCore.Mvc ControllerBase Class.** These controllers are filled with functions that act as API endpoints for the frontend to interact with.

## Structure and standards

Generally a controller class will contain the following things:
- Metadata such as package imports, namespace declerations, class headers.
    - A namespace declaration should follow the folder structure, so for a controller it should always be `backend.Controllers`.
    - Class and function headers are automaically parsed and loaded into the **Autodocs** section of the documentation, and should be kept as up to date as possible.
- An `[ApiController]` attribute
- A route attribute, such as `[Route("api/users")]`.
- A Regular class definition, including a service interface parameter, ensure that both the service and its accompanying interface are declared for dependancy injection in `Program.cs`.
    - Also ensure that this class inherits from the **AspNetCore.Mvc ControllerBase** class.
- A number of methods, each with their own:
    - Function header.
    - Http attribute, such as `[HttpGet("user/{id})]`.
    - The method should be of the async variety.
    - And should therefore return a `Task<ActionResult<{retType}>>`.
        - The ActionResult makes writing error codes more syntactically pleasing.
    - A Method should consist of a try-catch block, where:
        - Within the try:
            - Generally first a DTO will be unpacked into either raw data, a record, or a **Model**.
            - Then a **Service** following the same name should be awaited.
            - Finally an `Ok(result)` should be returned, where the result is usually the data that the service returns.
        - And within the catch:
            - The exception is Errored to the local console.
            - A Problem (http 500 response) is returned, optionally with a generic error message.

## Example

```csharp
using Microsoft.AspNetCore.Mvc;

using backend.Services;
using backend.Models;
using backend.Controllers.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;
/// <summary>
/// Controller for Creating reports on defective Items
/// </summary>
/// <param name="reportService">Report Service responsible for placing the report in the database</param>
[ApiController]
[Route("api/report")]

public class ReportController(IReportService reportService) : ControllerBase
{
    /// <summary>
    /// Create a report on an Item
    /// </summary>
    /// <param name="repDto">The ReportDTO containing the ItemId and the error found</param>
    /// <returns>A Problem(500) if something goes wrong, else a NoContent(204)</returns>
    [Authorize]
    [HttpPost("createReport")]
    public async Task<IActionResult> CreateReport(ReportDto repDto)
    {
        try
        {
            ItemError error = Enum.Parse<ItemError>(repDto.ItemError);
            await reportService.CreateReport(repDto.ItemId, error);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while creating report");
        }
    }
}
```