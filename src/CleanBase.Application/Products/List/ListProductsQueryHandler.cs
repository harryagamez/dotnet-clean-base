using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Domain.Common;
using CleanBase.Domain.Products;

namespace CleanBase.Application.Products.List;

internal sealed class ListProductsQueryHandler(IProductRepository productRepository)
    : IQueryHandler<ListProductsQuery, IReadOnlyList<ProductResponse>>
{
    public async Task<Result<IReadOnlyList<ProductResponse>>> Handle(
        ListProductsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> products = await productRepository.ListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ProductResponse>>([.. products.Select(ProductResponse.FromProduct)]);
    }
}
