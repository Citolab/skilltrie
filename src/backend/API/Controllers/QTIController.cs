/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using API.Services;
using API.Tools.Attributes;
using API.Tools.QTIConverting;
using Microsoft.AspNetCore.Mvc;
using API.Tools.MathMLConverter;

namespace API.Controllers;

/// <summary>
/// Controller for converting <see cref="Item"/>s to their QTI 3.0 format
/// Generating assessment.xml files.
/// </summary>
/// <param name="itemService">Item service responsible for getting items from the DB, see <see cref="ItemService"/></param>
/// <param name="mathMLConverter"> Converter </param>
[ApiController]
[Route("api/qti")]
public class QTIController(IItemService itemService, IMathMLConverter mathMLConverter) : ControllerBase
{
    // GET /api/qti/id
    /// <summary>
    /// Convert an <see cref="Item"/> to its QTI 3.0 format, given its Id.
    /// </summary>
    /// <param name="id">The item's Id</param>
    /// <returns></returns>
    [HttpGet("get-item/{id}")]
    [Produces("application/xml")]
    public async Task<ActionResult<string>> GetItem(int id)
    {
        try
        {
            var qtiItem = await itemService.GetItem(id, true);
            Response.ContentType = "text/xml";
            await mathMLConverter.ConvertItemsToMathML([qtiItem]);
            return Content(QTIXML.Convert(qtiItem));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while retrieving item");
        }
    }
    
    // GET /api/qti/id
    /// <summary>
    /// Returns the correct answer of an item in string format.
    /// </summary>
    /// <param name="id">The id of the item</param>
    [HttpGet("get-item/{id}/answer")]
    [DevelopmentOnly]
    public async Task<ActionResult<string>> GetItemCorrectAnswer(int id)
    {
        try
        {
            return await itemService.CorrectAnswer(id);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while retrieving item");
        }
    }

    // GET /api/qti/assessment
    /// <summary>
    /// Get an assessment.xml for a number of <see cref="Item"/>s, given their Ids.
    /// </summary>
    /// <param name="itemIds">The <see cref="Item"/> Ids</param>
    /// <returns></returns>
    [HttpPost("assessment")]
    [Produces("application/xml")]
    public async Task<ActionResult<string>> GetAssessment(int[] itemIds)
    {
        if (itemIds == null || itemIds.Length == 0)
            return BadRequest("Provide at last one item id within the array");

        try
        {
            Item[] items = await itemService.GetItemsForAssessment(itemIds);
            return Content(QTIXML.GenerateAssesment(items));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while retrieving assessment");
        }
    }
}
