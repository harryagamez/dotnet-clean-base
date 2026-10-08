using System.Diagnostics;
using CleanBase.Api.ErrorHandling;
using CleanBase.Api.Products;
using CleanBase.Application;
using CleanBase.Infrastructure;
using CleanBase.Infrastructure.Database;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions.TryAdd(
            "traceId",
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await app.Services.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");
app.MapProductEndpoints();

await app.RunAsync();
