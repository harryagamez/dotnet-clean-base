using CleanBase.Domain.Common;

namespace CleanBase.Domain.Products;

/// <summary>
/// Sample aggregate. Its state can only change through methods that enforce its invariants,
/// so an invalid <see cref="Product"/> can never exist in memory ("always valid" entity).
/// </summary>
public sealed class Product : Entity
{
    public const int NameMaxLength = 200;
    public const int SkuMaxLength = 50;

    private Product(Guid id, string name, string sku, decimal price)
        : base(id)
    {
        Name = name;
        Sku = sku;
        Price = price;
    }

    /// <summary>Parameterless constructor for EF Core materialization.</summary>
    private Product()
    {
        Name = string.Empty;
        Sku = string.Empty;
    }

    public string Name { get; private set; }

    /// <summary>Stock keeping unit. Unique and immutable; stored trimmed and in upper case.</summary>
    public string Sku { get; private set; }

    public decimal Price { get; private set; }

    public static Result<Product> Create(string name, string sku, decimal price)
    {
        Error? error = ValidateName(name) ?? ValidateSku(sku) ?? ValidatePrice(price);
        if (error is not null)
        {
            return error;
        }

        // Version 7 GUIDs are time-ordered, which keeps clustered indexes from fragmenting.
        return new Product(Guid.CreateVersion7(), name.Trim(), sku.Trim().ToUpperInvariant(), price);
    }

    public Result Update(string name, decimal price)
    {
        Error? error = ValidateName(name) ?? ValidatePrice(price);
        if (error is not null)
        {
            return Result.Failure(error);
        }

        Name = name.Trim();
        Price = price;
        return Result.Success();
    }

    private static Error? ValidateName(string name) =>
        string.IsNullOrWhiteSpace(name) ? ProductErrors.NameRequired
        : name.Trim().Length > NameMaxLength ? ProductErrors.NameTooLong
        : null;

    private static Error? ValidateSku(string sku) =>
        string.IsNullOrWhiteSpace(sku) ? ProductErrors.SkuRequired
        : sku.Trim().Length > SkuMaxLength ? ProductErrors.SkuTooLong
        : null;

    private static Error? ValidatePrice(decimal price) =>
        price < 0 ? ProductErrors.NegativePrice : null;
}
