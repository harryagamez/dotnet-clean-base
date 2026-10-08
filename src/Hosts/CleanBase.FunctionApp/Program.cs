using CleanBase.Application;
using CleanBase.FunctionApp.ErrorHandling;
using CleanBase.Infrastructure;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.UseMiddleware<ExceptionHandlingMiddleware>();

// Same registrations as the Web API: only the entry point changes, the core stays the same.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

await builder.Build().RunAsync();
