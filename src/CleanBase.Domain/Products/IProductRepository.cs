namespace CleanBase.Domain.Products;

/// <summary>
/// Persistence contract for <see cref="Product"/>. The Domain defines it and Infrastructure implements it
/// (Dependency Inversion: the inner layer owns the abstraction).
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken = default);

    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);

    void Add(Product product);

    void Remove(Product product);
}
