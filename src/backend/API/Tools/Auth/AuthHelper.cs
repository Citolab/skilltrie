/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace API.Tools.Auth;

public static class AuthHelper
{
    /// <summary>
    /// Checks whether for the current request a user is logged in and return the id of that user.
    /// </summary>
    /// <param name="controller">The current scoped controller.</param>
    /// <returns>The id of the current user logged in if any.</returns>
    /// <exception cref="BadHttpRequestException">When no authenticated user has been found.</exception>
    public static int GetAuthenticatedUserId(this ControllerBase controller)
    {
        var claim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claim is null) throw new BadHttpRequestException("No user authenticated", 401);
        return int.Parse(claim);
    }
}