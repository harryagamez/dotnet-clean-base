using CleanBase.Domain.Products;

namespace CleanBase.Application.Products;

/// <summary>Read model returned to callers. Entities never leave the Application layer.</summary>
public sealed record ProductResponse(Guid Id, string Name, string Sku, decimal Price)
{
    internal static ProductResponse FromProduct(Product product) =>
        new(product.Id, product.Name, product.Sku, product.Price);
}
