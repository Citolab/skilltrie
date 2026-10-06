/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.AspNetCore.Diagnostics;

namespace API.Handlers.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var statusCode = ex is BadHttpRequestException badRequest
            ? badRequest.StatusCode
            : 500;

        ctx.Response.StatusCode = statusCode;
        await ctx.Response.WriteAsJsonAsync(new
        {
            detail = ex.Message
        }, ct);

        return true;
    }
}
