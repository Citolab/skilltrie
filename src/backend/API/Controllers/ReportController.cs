/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers.DTOs;
using Models;
using API.Services;
using API.Tools.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;
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
            int userId = this.GetAuthenticatedUserId();
            await reportService.CreateReport(repDto.ItemId, error, userId);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while creating report");
        }
    }

    [HttpGet("getAllReports")]
    public async Task<IActionResult> GetAllReports()
    {
        try
        {
            var allReports = await reportService.GetAllReports();
            return Ok(allReports);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while creating report");
        }
    }
}
