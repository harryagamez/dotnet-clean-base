using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CleanBase.Api.ErrorHandling;

/// <summary>
/// Last line of defense for exceptions: logs them and answers with a ProblemDetails, so no stack trace
/// or internal detail ever reaches the client.
/// </summary>
internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problem;

        if (exception is BadHttpRequestException badRequest)
        {
            // Malformed input (e.g. invalid JSON). Minimal APIs throw it instead of answering 400 when
            // RouteHandlerOptions.ThrowOnBadRequest is on, which is the default in Development.
            LogBadRequest(logger, badRequest.Message);
            problem = new ProblemDetails
            {
                Status = badRequest.StatusCode,
                Detail = "The request is malformed or contains invalid values.",
            };
        }
        else
        {
            LogUnhandledException(logger, exception);
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server error",
                Detail = "An unexpected error occurred.",
            };
        }

        httpContext.Response.StatusCode = problem.Status.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem,
        });
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Bad request: {Reason}")]
    private static partial void LogBadRequest(ILogger logger, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);
}
