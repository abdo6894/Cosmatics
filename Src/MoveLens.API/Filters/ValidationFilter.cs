using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MoveLens.Api.Models;

namespace MoveLens.Api.Filters;

public sealed class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;

        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value!.Errors
                .Select(e => $"{kvp.Key}: {e.ErrorMessage}"))
            .ToList();

        context.Result = new BadRequestObjectResult(
            ApiResponse<object>.FailResponse("Validation failed.", errors));
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}