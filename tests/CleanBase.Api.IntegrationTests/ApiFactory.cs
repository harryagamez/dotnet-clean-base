using CleanBase.Api.IntegrationTests;
using CleanBase.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MsSql;

// One API instance and one database for the whole test assembly: starting SQL Server is the slow part.
[assembly: AssemblyFixture(typeof(ApiFactory))]

namespace CleanBase.Api.IntegrationTests;

/// <summary>
/// Hosts the real API in memory against a real SQL Server.
/// By default it starts a disposable SQL Server container (requires Docker). If the
/// <see cref="ConnectionStringVariable"/> environment variable is set, it uses that database instead.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ConnectionStringVariable = "INTEGRATION_TESTS_DATABASE";

    private MsSqlContainer? _container;
    private string _connectionString = string.Empty;

    public async ValueTask InitializeAsync()
    {
        string? externalConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);

        if (string.IsNullOrWhiteSpace(externalConnectionString))
        {
            _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
            await _container.StartAsync();
            _connectionString = _container.GetConnectionString();
        }
        else
        {
            _connectionString = externalConnectionString;
        }

        // Accessing Services builds the host, so the connection string must be set before this line.
        await Services.ApplyMigrationsAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Database", _connectionString);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
