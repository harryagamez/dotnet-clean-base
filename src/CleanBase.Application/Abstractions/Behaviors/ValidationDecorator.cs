using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Domain.Common;
using FluentValidation;
using FluentValidation.Results;

namespace CleanBase.Application.Abstractions.Behaviors;

/// <summary>
/// Runs every FluentValidation validator registered for a command before its handler.
/// An invalid command never reaches the handler: the decorator returns a <see cref="ValidationError"/> instead.
/// </summary>
internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            ValidationError? error = await ValidateAsync(command, validators, cancellationToken);

            return error is null
                ? await innerHandler.Handle(command, cancellationToken)
                : Result.Failure<TResponse>(error);
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            ValidationError? error = await ValidateAsync(command, validators, cancellationToken);

            return error is null
                ? await innerHandler.Handle(command, cancellationToken)
                : Result.Failure(error);
        }
    }

    private static async Task<ValidationError?> ValidateAsync<TCommand>(
        TCommand command,
        IEnumerable<IValidator<TCommand>> validators,
        CancellationToken cancellationToken)
    {
        IValidator<TCommand>[] validatorArray = [.. validators];
        if (validatorArray.Length == 0)
        {
            return null;
        }

        var context = new ValidationContext<TCommand>(command);
        ValidationResult[] results = await Task.WhenAll(
            validatorArray.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        Error[] errors =
        [
            .. results
                .SelectMany(result => result.Errors)
                .Select(failure => Error.Validation(failure.PropertyName, failure.ErrorMessage)),
        ];

        return errors.Length == 0 ? null : new ValidationError(errors);
    }
}
