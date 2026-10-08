using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Domain.Common;
using CleanBase.Domain.Products;

namespace CleanBase.Application.Products.GetById;

internal sealed class GetProductByIdQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetByIdAsync(query.Id, cancellationToken);

        return product is null
            ? ProductErrors.NotFound(query.Id)
            : ProductResponse.FromProduct(product);
    }
}
