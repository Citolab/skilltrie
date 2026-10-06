﻿/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.NewItemsAlgorithm;
using API.Services;
using API.Tools.EventQueue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Models;
using System.Net;

namespace API.Controllers;

[ApiController]
[Route("api/ai")]
public class AIController(IAIService aiService) : ControllerBase
{
    /// <summary>
    /// Creates a sample question via AI, used for development purposes
    /// </summary>
    /// <returns>An <see cref="OkObjectResult"/> containing the generated <see cref="Item"/> on success. Returns an <see cref="ObjectResult"/> with the status code 500 on failure.</returns>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("createSampleQuestion")]
    public async Task<ActionResult<Item>> MakeQuestions(int topicId = 0)
    {
        try
        {
            var question = (await aiService.MakeQuestions(topicId, 1)).FirstOrDefault<Item>();
            return Ok(question);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An Error occurred while creating a sample question");
        }
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("createSampleQuestion/{topicId}")]
    public async Task<ActionResult<Item>> MakeQuestionsWithId(int topicId)
    {
        return await MakeQuestions(topicId);
    }

    [HttpGet("createSampleQuestions/{topicId}/{amount}")]
    public async Task<ActionResult> MakeNumQuestions(int topicId = 0, int amount = 1)
    {
        try
        {
            var question = await aiService.MakeQuestions(topicId, amount);
            return Ok(question);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return Problem("An Error occurred while creating a sample question");
        }
    }

    /// <summary>
    /// Creates Feedback for a test via AI
    /// </summary>
    /// <returns>Multiple <see cref="Response"/> containing a part on the AI response content stream.</returns>
    [Authorize]
    [EnableRateLimiting("AI-rate-limiter")]
    [HttpGet("feedback/{testId}")]
    public async Task CreateFeedBack(int testId)
    {
        Response.ContentType = "text/plain";
        Response.Headers.CacheControl = "no-cache";
        try
        {
            await foreach (var token in aiService.AiFeedback(testId))
            {
                await Response.WriteAsync(token);
                await Response.Body.FlushAsync();
            }
        }
        catch
        {
            await Response.WriteAsync("An Error occurred while creating feedback");
            await Response.Body.FlushAsync();
            return;
        }

    }

}
