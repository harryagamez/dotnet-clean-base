using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Application.Products.Create;
using CleanBase.Domain.Common;
using CleanBase.Domain.Products;
using CleanBase.FunctionApp.Functions.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CleanBase.FunctionApp.UnitTests.Products;

/// <summary>
/// Functions are plain classes, so they can be tested without the Functions runtime:
/// build an HttpRequest, call the method and inspect the IActionResult.
/// </summary>
public sealed class CreateProductFunctionTests
{
    private readonly ICommandHandler<CreateProductCommand, Guid> _handler =
        Substitute.For<ICommandHandler<CreateProductCommand, Guid>>();

    private readonly CreateProductFunction _function;
    private readonly HttpRequest _request;

    public CreateProductFunctionTests()
    {
        _function = new CreateProductFunction(_handler);
        _request = new DefaultHttpContext().Request;
        _request.Path = "/api/products";
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task RunAsync_WhenHandlerSucceeds_Returns201WithLocation()
    {
        var id = Guid.NewGuid();
        _handler.Handle(Arg.Any<CreateProductCommand>(), Arg.Any<CancellationToken>()).Returns(Result.Success(id));

        IActionResult result = await _function.RunAsync(
            _request,
            new CreateProductRequest("Keyboard", "KB-001", 10m),
            CancellationToken);

        CreatedResult created = result.ShouldBeOfType<CreatedResult>();
        created.Location.ShouldBe($"/api/products/{id}");
        created.Value.ShouldBe(id);
    }

    [Fact]
    public async Task RunAsync_WithValidationError_Returns400ValidationProblem()
    {
        var error = new ValidationError([Error.Validation("Name", "'Name' must not be empty.")]);
        _handler.Handle(Arg.Any<CreateProductCommand>(), Arg.Any<CancellationToken>()).Returns(Result.Failure<Guid>(error));

        IActionResult result = await _function.RunAsync(
            _request,
            new CreateProductRequest(string.Empty, "KB-001", 10m),
            CancellationToken);

        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        ValidationProblemDetails problem = objectResult.Value.ShouldBeOfType<ValidationProblemDetails>();
        problem.Errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public async Task RunAsync_WithConflict_Returns409()
    {
        _handler.Handle(Arg.Any<CreateProductCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(ProductErrors.SkuAlreadyExists));

        IActionResult result = await _function.RunAsync(
            _request,
            new CreateProductRequest("Keyboard", "KB-001", 10m),
            CancellationToken);

        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task RunAsync_WithoutBody_Returns400WithoutCallingHandler()
    {
        IActionResult result = await _function.RunAsync(_request, null, CancellationToken);

        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        await _handler.DidNotReceive().Handle(Arg.Any<CreateProductCommand>(), Arg.Any<CancellationToken>());
    }
}
