using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Domain.Common;
using Microsoft.Extensions.Logging;

namespace CleanBase.Application.Abstractions.Behaviors;

/// <summary>
/// Logs the start and the outcome of every command and query. Business failures are logged
/// as warnings; unexpected exceptions keep propagating to the host's global handler.
/// </summary>
internal static partial class LoggingDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken) =>
            HandleWithLoggingAsync(logger, typeof(TCommand).Name, () => innerHandler.Handle(command, cancellationToken));
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandBaseHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public Task<Result> Handle(TCommand command, CancellationToken cancellationToken) =>
            HandleWithLoggingAsync(logger, typeof(TCommand).Name, () => innerHandler.Handle(command, cancellationToken));
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken) =>
            HandleWithLoggingAsync(logger, typeof(TQuery).Name, () => innerHandler.Handle(query, cancellationToken));
    }

    private static async Task<TResult> HandleWithLoggingAsync<TResult>(
        ILogger logger,
        string requestName,
        Func<Task<TResult>> handle)
        where TResult : Result
    {
        LogProcessing(logger, requestName);

        TResult result = await handle();

        if (result.IsSuccess)
        {
            LogCompleted(logger, requestName);
        }
        else
        {
            LogFailed(logger, requestName, result.Error.Code);
        }

        return result;
    }

    // [LoggerMessage] generates the logging code at compile time: no boxing and no template parsing per call.
    [LoggerMessage(Level = LogLevel.Information, Message = "Processing {RequestName}")]
    private static partial void LogProcessing(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Completed {RequestName}")]
    private static partial void LogCompleted(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Completed {RequestName} with error {ErrorCode}")]
    private static partial void LogFailed(ILogger logger, string requestName, string errorCode);
}
