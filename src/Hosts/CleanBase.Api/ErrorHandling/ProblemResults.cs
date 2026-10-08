using CleanBase.Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanBase.Api.ErrorHandling;

/// <summary>Translates a business <see cref="Error"/> into an RFC 9457 ProblemDetails response.</summary>
internal static class ProblemResults
{
    public static ProblemHttpResult ToProblem(this Error error)
    {
        if (error is ValidationError validationError)
        {
            // Same shape as ASP.NET Core's built-in validation responses: { "errors": { "field": ["message"] } }.
            Dictionary<string, string[]> errors = validationError.Errors
                .GroupBy(fieldError => fieldError.Code)
                .ToDictionary(group => group.Key, group => group.Select(fieldError => fieldError.Description).ToArray());

            return TypedResults.Problem(new HttpValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = validationError.Description,
                Extensions = { ["code"] = validationError.Code },
            });
        }

        return TypedResults.Problem(
            statusCode: GetStatusCode(error.Type),
            detail: error.Description,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };
}
