/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Tools.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class DevelopmentOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (Environment.GetEnvironmentVariable("BACKEND_ENVIRONMENT") != "Development")
            context.Result = new NotFoundResult();
        
        base.OnActionExecuting(context);
    }
}