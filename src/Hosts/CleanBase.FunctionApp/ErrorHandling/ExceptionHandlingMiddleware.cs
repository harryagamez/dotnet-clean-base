using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace CleanBase.FunctionApp.ErrorHandling;

/// <summary>
/// Functions equivalent of the Web API's global exception handler. For HTTP triggers it answers
/// with a ProblemDetails (400 when the input can't be bound, 500 otherwise). Non-HTTP triggers
/// rethrow so the runtime applies its own retry policies.
/// </summary>
internal sealed partial class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (FunctionInputConverterException) when (context.GetHttpContext() is { } httpContext)
        {
            // The input binding failed (e.g. malformed JSON body): it's the client's fault, not ours.
            await WriteProblemAsync(
                httpContext,
                StatusCodes.Status400BadRequest,
                "The request body is missing or isn't valid JSON.",
                context);
        }
        catch (Exception exception)
        {
            LogUnhandledException(logger, context.FunctionDefinition.Name, exception);

            HttpContext? httpContext = context.GetHttpContext();
            if (httpContext is null)
            {
                throw;
            }

            await WriteProblemAsync(
                httpContext,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                context);
        }
    }

    private static Task WriteProblemAsync(HttpContext httpContext, int statusCode, string detail, FunctionContext context)
    {
        httpContext.Response.StatusCode = statusCode;

        return httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Detail = detail,
                Extensions = { ["traceId"] = context.InvocationId },
            },
            options: null,
            contentType: "application/problem+json",
            cancellationToken: context.CancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in function {FunctionName}")]
    private static partial void LogUnhandledException(ILogger logger, string functionName, Exception exception);
}
