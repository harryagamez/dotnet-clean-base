using CleanBase.Application.Abstractions.Messaging;

namespace CleanBase.Application.Products.Delete;

public sealed record DeleteProductCommand(Guid Id) : ICommand;
