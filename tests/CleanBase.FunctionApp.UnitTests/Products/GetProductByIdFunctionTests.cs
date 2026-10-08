using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Application.Products;
using CleanBase.Application.Products.GetById;
using CleanBase.Domain.Products;
using CleanBase.FunctionApp.Functions.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CleanBase.FunctionApp.UnitTests.Products;

public sealed class GetProductByIdFunctionTests
{
    private readonly IQueryHandler<GetProductByIdQuery, ProductResponse> _handler =
        Substitute.For<IQueryHandler<GetProductByIdQuery, ProductResponse>>();

    private readonly GetProductByIdFunction _function;

    public GetProductByIdFunctionTests()
    {
        _function = new GetProductByIdFunction(_handler);
    }

    [Fact]
    public async Task RunAsync_WhenProductExists_Returns200WithProduct()
    {
        var product = new ProductResponse(Guid.NewGuid(), "Keyboard", "KB-001", 10m);
        _handler.Handle(new GetProductByIdQuery(product.Id), Arg.Any<CancellationToken>()).Returns(product);

        IActionResult result = await _function.RunAsync(
            new DefaultHttpContext().Request,
            product.Id,
            TestContext.Current.CancellationToken);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(product);
    }

    [Fact]
    public async Task RunAsync_WhenProductDoesNotExist_Returns404Problem()
    {
        var id = Guid.NewGuid();
        _handler.Handle(new GetProductByIdQuery(id), Arg.Any<CancellationToken>()).Returns(ProductErrors.NotFound(id));

        IActionResult result = await _function.RunAsync(
            new DefaultHttpContext().Request,
            id,
            TestContext.Current.CancellationToken);

        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        objectResult.Value.ShouldBeOfType<ProblemDetails>().Extensions["code"].ShouldBe("Products.NotFound");
    }
}
