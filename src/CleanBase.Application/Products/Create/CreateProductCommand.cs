using CleanBase.Application.Abstractions.Messaging;

namespace CleanBase.Application.Products.Create;

/// <summary>Creates a product and returns its Id.</summary>
public sealed record CreateProductCommand(string Name, string Sku, decimal Price) : ICommand<Guid>;
