using CleanBase.Application.Abstractions.Data;
using CleanBase.Application.Products.Create;
using CleanBase.Domain.Common;
using CleanBase.Domain.Products;
using NSubstitute;

namespace CleanBase.Application.UnitTests.Products;

public sealed class CreateProductCommandHandlerTests
{
    private static readonly CreateProductCommand Command = new("Keyboard", "kb-001", 89.90m);

    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(_productRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenSkuIsAvailable_AddsProductAndSaves()
    {
        Result<Guid> result = await _handler.Handle(Command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _productRepository.Received(1).Add(Arg.Is<Product>(product =>
            product.Id == result.Value && product.Sku == "KB-001"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ChecksUniquenessAgainstNormalizedSku()
    {
        await _handler.Handle(Command, TestContext.Current.CancellationToken);

        await _productRepository.Received(1).SkuExistsAsync("KB-001", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSkuAlreadyExists_ReturnsConflictAndDoesNotSave()
    {
        _productRepository.SkuExistsAsync("KB-001", Arg.Any<CancellationToken>()).Returns(true);

        Result<Guid> result = await _handler.Handle(Command, TestContext.Current.CancellationToken);

        result.Error.ShouldBe(ProductErrors.SkuAlreadyExists);
        _productRepository.DidNotReceive().Add(Arg.Any<Product>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainRejectsData_ReturnsDomainError()
    {
        Result<Guid> result = await _handler.Handle(
            Command with { Price = -1m },
            TestContext.Current.CancellationToken);

        result.Error.ShouldBe(ProductErrors.NegativePrice);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
