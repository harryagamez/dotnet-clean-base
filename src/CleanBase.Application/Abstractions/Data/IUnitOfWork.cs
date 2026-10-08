namespace CleanBase.Application.Abstractions.Data;

/// <summary>
/// Commits every change tracked during the current operation as a single unit.
/// Repositories only stage changes; the handler decides when to save.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
