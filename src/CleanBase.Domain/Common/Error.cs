namespace CleanBase.Domain.Common;

/// <summary>
/// An expected business error. Errors are values, not exceptions: exceptions are reserved
/// for unexpected situations (bugs, infrastructure failures).
/// </summary>
/// <param name="Code">Stable, machine-readable identifier, e.g. <c>Products.NotFound</c>.</param>
/// <param name="Description">Human-readable explanation.</param>
/// <param name="Type">Category used to translate the error at the edges.</param>
public record Error(string Code, string Description, ErrorType Type)
{
    /// <summary>Represents the absence of an error (used by successful results).</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
}
