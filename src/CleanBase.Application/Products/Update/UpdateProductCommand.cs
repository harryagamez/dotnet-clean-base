using CleanBase.Application.Abstractions.Messaging;

namespace CleanBase.Application.Products.Update;

/// <summary>Changes a product's name and price. The SKU is immutable.</summary>
public sealed record UpdateProductCommand(Guid Id, string Name, decimal Price) : ICommand;
