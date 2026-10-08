namespace CleanBase.Domain.Common;

/// <summary>
/// Groups several validation errors (one per invalid field) into a single <see cref="Error"/>.
/// Each inner error uses the field name as its <see cref="Error.Code"/>.
/// </summary>
public sealed record ValidationError(IReadOnlyCollection<Error> Errors)
    : Error("Validation.General", "One or more validation errors occurred.", ErrorType.Validation);
