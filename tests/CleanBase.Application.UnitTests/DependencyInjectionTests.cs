using CleanBase.Application.Abstractions.Behaviors;
using CleanBase.Application.Abstractions.Data;
using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Application.Products;
using CleanBase.Application.Products.Create;
using CleanBase.Application.Products.Delete;
using CleanBase.Application.Products.GetById;
using CleanBase.Domain.Products;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CleanBase.Application.UnitTests;

/// <summary>Verifies the handler registration and the decorator chain (Logging -> Validation -> Handler).</summary>
public sealed class DependencyInjectionTests : IDisposable
{
    private readonly ServiceProvider _provider;

    public DependencyInjectionTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped(_ => Substitute.For<IProductRepository>());
        services.AddScoped(_ => Substitute.For<IUnitOfWork>());
        services.AddApplication();

        _provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
    }

    [Fact]
    public void CommandHandlerWithResponse_IsWrappedByLoggingThenValidation()
    {
        using IServiceScope scope = _provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateProductCommand, Guid>>();

        handler.ShouldBeOfType<LoggingDecorator.CommandHandler<CreateProductCommand, Guid>>();
    }

    [Fact]
    public void CommandHandlerWithoutResponse_IsWrappedByLogging()
    {
        using IServiceScope scope = _provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<DeleteProductCommand>>();

        handler.ShouldBeOfType<LoggingDecorator.CommandBaseHandler<DeleteProductCommand>>();
    }

    [Fact]
    public void QueryHandler_IsWrappedByLogging()
    {
        using IServiceScope scope = _provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetProductByIdQuery, ProductResponse>>();

        handler.ShouldBeOfType<LoggingDecorator.QueryHandler<GetProductByIdQuery, ProductResponse>>();
    }

    public void Dispose() => _provider.Dispose();
}
