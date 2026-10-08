using CleanBase.Domain.Common;

namespace CleanBase.Domain.Products;

/// <summary>Catalog of the errors a <see cref="Product"/> operation can produce.</summary>
public static class ProductErrors
{
    public static readonly Error NameRequired =
        Error.Validation("Products.NameRequired", "The product name is required.");

    public static readonly Error NameTooLong =
        Error.Validation("Products.NameTooLong", $"The product name can't exceed {Product.NameMaxLength} characters.");

    public static readonly Error SkuRequired =
        Error.Validation("Products.SkuRequired", "The product SKU is required.");

    public static readonly Error SkuTooLong =
        Error.Validation("Products.SkuTooLong", $"The product SKU can't exceed {Product.SkuMaxLength} characters.");

    public static readonly Error NegativePrice =
        Error.Validation("Products.NegativePrice", "The product price can't be negative.");

    public static readonly Error SkuAlreadyExists =
        Error.Conflict("Products.SkuAlreadyExists", "Another product already uses this SKU.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("Products.NotFound", $"The product with id '{id}' was not found.");
}
