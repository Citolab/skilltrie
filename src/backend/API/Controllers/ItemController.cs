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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

///<summary>
/// Item Controller: defines endpoints for retrieving items from the database,
/// modifying items, adding item topics, activating/deactivating items and searching
/// for items.
///</summary>
[ApiController]
[Route("api/items")]
public class ItemController(IItemService itemService) : ControllerBase
{
    private const int MaxQuestionBodyLength = 100;

    /// <summary>
    /// Retrieve a range of items from the database.
    /// </summary>
    /// <param name="offset">the id of an item from which we want to begin fetching</param>
    /// <param name="range">the number of items to fetch from the offset</param>
    /// <param name="sortColumn">the property to sort by</param>
    /// <param name="sortOrder">ascending or descending</param>
    /// <returns>an array of <see cref="SmallItemDto"/></returns>
    [HttpGet("items")]
    public async Task<ActionResult<SmallItemDto[]>> GetItems(
        int offset = 0,
        int range = 50,
        string sortColumn = "id",
        string sortOrder = "ascending")
    {
        try
        {
            range = Math.Clamp(range, 1, 50);
            offset = Math.Max(0, offset);

            var items = await itemService.GetItems(offset, range, MaxQuestionBodyLength, sortColumn, sortOrder);

            var dto = items.Select(item => new SmallItemDto
            {
                Id = item.Id,
                Type = item.Type,
                Active = item.Active,
                QuestionText = item.QuestionText,
                Level = item.Level,
                Lang = item.Lang,
                ResponseType = item.ResponseType,
                AppearanceCount = item.AppearanceCount,
                Source = item.Source,
                Topics = item.Scopes
            }).ToArray();

            return Ok(dto);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An error occured while retrieving items");
        }
    }

    /// <summary>
    /// Retrieve specific item via its id
    /// </summary>
    /// <param name="id">the item's id</param>
    /// <returns>the specific ItemDto if the Item exists</returns>
    [HttpGet("item/{id}")]
    public async Task<ActionResult<ItemDto>> GetItem(int id)
    {
        try
        {
            var item = await itemService.GetItem(id, true, true);
            ItemDto itemDto = ItemDto.CreateItemDto(item);
            return Ok(itemDto);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("Item not found");
        }
    }

    /// <summary>
    /// Update a specific Item, the id of the item that needs updating
    /// is extracted from the JSON body
    /// </summary>
    /// <param name="requestItem">the JSON body representing an <see cref="ItemDto"/></param>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("update")]
    public async Task<ActionResult<ItemDto>> UpdateItem(ItemDto requestItem)
    {
        try
        {
            var incomingItem = ItemDto.FromItemDto(requestItem);
            Item UpdatedItem = await itemService.UpdateItem(incomingItem);
            return Ok(UpdatedItem);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("Error occured while updating item");
        }
    }

    /// <summary>
    /// Search for a specific item by its question text.
    /// </summary>
    /// <param name="searchString">the (sub)string of a possible question text to search for</param>
    /// <returns>an array of relevant <see cref="SmallItemDto"/>'s that match the search string</returns>
    [HttpGet("searchbyquestiontext")]
    public async Task<ActionResult<SmallItemDto[]>> SearchByQuestionText(string searchString)
    {
        try
        {
            var items = await itemService.GetItemByAnswertext(searchString, MaxQuestionBodyLength);
            return Ok(items);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("Error occured while searching item");
        }
    }

    /// <summary>
    /// Deactivate an item given its id
    /// </summary>
    /// <param name="id">the item's id</param>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("deactivate/{id}")]
    public async Task<IActionResult> DeactivateItem(int id)
    {
        try
        {
            var item = await itemService.DeactivateItem(id);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("Error occured while deactivating item");
        }
    }

    /// <summary>
    /// Activate an item given its id
    /// </summary>
    /// <param name="id">the item's id</param>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("activate/{id}")]
    public async Task<IActionResult> ReactivateItem(int id)
    {
        try
        {
            var item = await itemService.ActivateItem(id);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("Error occured while activating item");
        }
    }

    [HttpGet("resolve/{id}")]
    public async Task<IActionResult> ResolveReports(int id)
    {
        try
        {
            await itemService.ResolveReports(id);
            return NoContent();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("Error occured while resolving reports");
        }
    }
}
