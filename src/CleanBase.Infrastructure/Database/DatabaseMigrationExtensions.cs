using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanBase.Infrastructure.Database;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations. Meant for local development and tests; in production,
    /// run migrations as a deployment step (e.g. an idempotent script or a migration bundle).
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
