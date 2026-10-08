using CleanBase.Api.ErrorHandling;
using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Application.Products;
using CleanBase.Application.Products.Create;
using CleanBase.Application.Products.Delete;
using CleanBase.Application.Products.GetById;
using CleanBase.Application.Products.List;
using CleanBase.Application.Products.Update;
using CleanBase.Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanBase.Api.Products;

/// <summary>
/// Products endpoints. Each one only translates HTTP into a command or query and the result back into HTTP:
/// all the logic lives in the handlers. The typed results (Results&lt;...&gt;) also document the responses in OpenAPI.
/// </summary>
internal static class ProductEndpoints
{
    private const string GetByIdRouteName = "GetProductById";

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/products").WithTags("Products");

        group.MapPost("/", CreateAsync);
        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync).WithName(GetByIdRouteName);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return app;
    }

    private static async Task<Results<CreatedAtRoute<Guid>, ProblemHttpResult>> CreateAsync(
        CreateProductRequest request,
        ICommandHandler<CreateProductCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        Result<Guid> result = await handler.Handle(
            new CreateProductCommand(request.Name, request.Sku, request.Price),
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, GetByIdRouteName, new { id = result.Value })
            : result.Error.ToProblem();
    }

    private static async Task<Results<Ok<IReadOnlyList<ProductResponse>>, ProblemHttpResult>> ListAsync(
        IQueryHandler<ListProductsQuery, IReadOnlyList<ProductResponse>> handler,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<ProductResponse>> result = await handler.Handle(new ListProductsQuery(), cancellationToken);

        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }

    private static async Task<Results<Ok<ProductResponse>, ProblemHttpResult>> GetByIdAsync(
        Guid id,
        IQueryHandler<GetProductByIdQuery, ProductResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<ProductResponse> result = await handler.Handle(new GetProductByIdQuery(id), cancellationToken);

        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        ICommandHandler<UpdateProductCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(
            new UpdateProductCommand(id, request.Name, request.Price),
            cancellationToken);

        return result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteAsync(
        Guid id,
        ICommandHandler<DeleteProductCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeleteProductCommand(id), cancellationToken);

        return result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();
    }
}
