using CleanBase.Application.Abstractions.Messaging;

namespace CleanBase.Application.Products.List;

public sealed record ListProductsQuery : IQuery<IReadOnlyList<ProductResponse>>;
