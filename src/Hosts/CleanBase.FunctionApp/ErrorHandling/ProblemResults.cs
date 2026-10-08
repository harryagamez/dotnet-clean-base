using CleanBase.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanBase.FunctionApp.ErrorHandling;

/// <summary>
/// Translates a business <see cref="Error"/> into an RFC 9457 ProblemDetails response.
/// Mirrors the Web API mapping, but returns <see cref="IActionResult"/>, the result type Functions uses.
/// </summary>
internal static class ProblemResults
{
    public static ObjectResult ToProblem(this Error error)
    {
        ProblemDetails problem;

        if (error is ValidationError validationError)
        {
            Dictionary<string, string[]> errors = validationError.Errors
                .GroupBy(fieldError => fieldError.Code)
                .ToDictionary(group => group.Key, group => group.Select(fieldError => fieldError.Description).ToArray());

            problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = validationError.Description,
            };
        }
        else
        {
            problem = new ProblemDetails
            {
                Status = GetStatusCode(error.Type),
                Detail = error.Description,
            };
        }

        problem.Extensions["code"] = error.Code;

        return ToObjectResult(problem);
    }

    /// <summary>
    /// 400 for a request without a body. Minimal APIs answer this way on their own; in Functions
    /// an empty body binds as null, so each function checks it explicitly.
    /// </summary>
    public static ObjectResult MissingBody() => ToObjectResult(new ProblemDetails
    {
        Status = StatusCodes.Status400BadRequest,
        Detail = "The request body is required.",
    });

    private static ObjectResult ToObjectResult(ProblemDetails problem)
    {
        var result = new ObjectResult(problem) { StatusCode = problem.Status };
        result.ContentTypes.Add("application/problem+json");
        return result;
    }

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };
}
