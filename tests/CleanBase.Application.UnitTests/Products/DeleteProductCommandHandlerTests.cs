using CleanBase.Application.Abstractions.Data;
using CleanBase.Application.Products.Delete;
using CleanBase.Domain.Common;
using CleanBase.Domain.Products;
using NSubstitute;

namespace CleanBase.Application.UnitTests.Products;

public sealed class DeleteProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _handler = new DeleteProductCommandHandler(_productRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenProductExists_RemovesAndSaves()
    {
        Product product = Product.Create("Keyboard", "KB-001", 10m).Value;
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        Result result = await _handler.Handle(
            new DeleteProductCommand(product.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _productRepository.Received(1).Remove(product);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        Result result = await _handler.Handle(new DeleteProductCommand(id), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(ProductErrors.NotFound(id));
        _productRepository.DidNotReceive().Remove(Arg.Any<Product>());
    }
}
