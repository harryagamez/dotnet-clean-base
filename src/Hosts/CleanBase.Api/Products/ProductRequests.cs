namespace CleanBase.Api.Products;

// HTTP contracts. They are separate from the commands so the API can evolve
// (versioning, renamed fields) without touching the Application layer.

internal sealed record CreateProductRequest(string Name, string Sku, decimal Price);

internal sealed record UpdateProductRequest(string Name, decimal Price);
