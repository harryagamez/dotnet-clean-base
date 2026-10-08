using CleanBase.Application.Abstractions.Data;
using CleanBase.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace CleanBase.Infrastructure.Database;

/// <summary>
/// EF Core context. It also implements <see cref="IUnitOfWork"/>: the DbContext already tracks changes
/// and saves them in one transaction, so a separate wrapper class would add nothing.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration<T> in this assembly (one class per entity).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
