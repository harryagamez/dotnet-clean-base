namespace CleanBase.Domain.Common;

/// <summary>
/// Category of an <see cref="Error"/>. Hosts use it to pick the transport-specific
/// representation (for HTTP: 400, 404, 409, 500...).
/// </summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
}
