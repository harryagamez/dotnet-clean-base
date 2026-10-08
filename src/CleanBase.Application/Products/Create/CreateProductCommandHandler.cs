using CleanBase.Application.Abstractions.Data;
using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Domain.Common;
using CleanBase.Domain.Products;

namespace CleanBase.Application.Products.Create;

internal sealed class CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        Result<Product> productResult = Product.Create(command.Name, command.Sku, command.Price);
        if (productResult.IsFailure)
        {
            return productResult.Error;
        }

        Product product = productResult.Value;

        // Checked against the normalized SKU. The unique index in the database is the final guarantee.
        if (await productRepository.SkuExistsAsync(product.Sku, cancellationToken))
        {
            return ProductErrors.SkuAlreadyExists;
        }

        productRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
