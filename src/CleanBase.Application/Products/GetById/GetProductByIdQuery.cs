using CleanBase.Application.Abstractions.Messaging;

namespace CleanBase.Application.Products.GetById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductResponse>;
