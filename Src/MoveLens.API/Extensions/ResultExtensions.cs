using Microsoft.AspNetCore.Mvc;
using MoveLens.Api.Models;
using MoveLens.Domain.Common.Results;
using DomainError = MoveLens.Domain.Common.Results.Error;

namespace MoveLens.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(
                ApiResponse<T>.SuccessResponse(result.Value));

        return ToHttpResult(result.TopError.Type, result.Errors);
    }

    public static IActionResult ToActionResult<T>(
        this   Result<T>       result,
        Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess(result.Value);

        return ToHttpResult(result.TopError.Type, result.Errors);
    }

    private static IActionResult ToHttpResult(ErrorKind kind, IEnumerable<DomainError> errors)
    {
        var errorList = errors.ToList();
        var messages  = errorList.Select(x => $"{x.Code}: {x.Description}").ToList();
        var message   = errorList.Count > 0 ? errorList[0].Description : "An error occurred.";
        var body      = ApiResponse<object>.FailResponse(message, messages);

        return kind switch
        {
            ErrorKind.Validation   => new BadRequestObjectResult(body),
            ErrorKind.Unauthorized => new UnauthorizedObjectResult(body),
            ErrorKind.Forbidden    => new ObjectResult(body) { StatusCode = 403 },
            ErrorKind.NotFound     => new NotFoundObjectResult(body),
            ErrorKind.Conflict     => new ConflictObjectResult(body),
            ErrorKind.Failure      => new ObjectResult(body) { StatusCode = 422 },
            _                      => new ObjectResult(body) { StatusCode = 500 },
        };
    }
}