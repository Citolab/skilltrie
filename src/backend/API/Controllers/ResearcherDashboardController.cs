/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Text.RegularExpressions;
using API.Controllers.DTOs;
using Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Versioning;

namespace API.Controllers;

/// <summary>
/// Controller for querying metrics to display to the researcher 
/// </summary>
/// <param name="ResearcherDashboardService"> Service responsible for querying data from the database</param>
[ApiController]
[Route("api/researcher/dashboard")]
public class ResearcherDashboardController
    (IResearcherDashboardService ResearcherDashboardService) : ControllerBase
{
    /// <summary>
    /// Create the metrics to display on the researcher dashboard
    /// </summary>
    /// <returns>A Problem(500) if something goes wrong, else an Ok with the corresponding metrics</returns>
    [HttpGet("metrics")]
    public async Task<ActionResult<MetricDTO>> getMetrics()
    {
        try
        {
            MetricDTO metrics = await ResearcherDashboardService.getMetrics();
            return Ok(metrics);
        }
        catch(Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}