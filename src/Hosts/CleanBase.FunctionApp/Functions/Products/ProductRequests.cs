namespace CleanBase.FunctionApp.Functions.Products;

// HTTP contracts. They are separate from the commands so the API can evolve
// (versioning, renamed fields) without touching the Application layer.

public sealed record CreateProductRequest(string Name, string Sku, decimal Price);

public sealed record UpdateProductRequest(string Name, decimal Price);
