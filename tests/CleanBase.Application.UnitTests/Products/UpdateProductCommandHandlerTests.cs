using CleanBase.Application.Abstractions.Data;
using CleanBase.Application.Products.Update;
using CleanBase.Domain.Common;
using CleanBase.Domain.Products;
using NSubstitute;

namespace CleanBase.Application.UnitTests.Products;

public sealed class UpdateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _handler = new UpdateProductCommandHandler(_productRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenProductExists_UpdatesAndSaves()
    {
        Product product = Product.Create("Keyboard", "KB-001", 10m).Value;
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        Result result = await _handler.Handle(
            new UpdateProductCommand(product.Id, "Mouse", 20m),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        product.Name.ShouldBe("Mouse");
        product.Price.ShouldBe(20m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        Result result = await _handler.Handle(
            new UpdateProductCommand(id, "Mouse", 20m),
            TestContext.Current.CancellationToken);

        result.Error.ShouldBe(ProductErrors.NotFound(id));
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
