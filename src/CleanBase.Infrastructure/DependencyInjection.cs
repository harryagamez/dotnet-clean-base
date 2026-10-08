using CleanBase.Application.Abstractions.Data;
using CleanBase.Domain.Products;
using CleanBase.Infrastructure.Database;
using CleanBase.Infrastructure.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanBase.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("The connection string 'Database' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
            connectionString,
            sqlServer => sqlServer.EnableRetryOnFailure()));

        // The same scoped DbContext instance serves as the unit of work for the whole request.
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

        return services;
    }
}
