using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Application.Products;
using CleanBase.Application.Products.List;
using CleanBase.Domain.Common;
using CleanBase.FunctionApp.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CleanBase.FunctionApp.Functions.Products;

public sealed class ListProductsFunction(IQueryHandler<ListProductsQuery, IReadOnlyList<ProductResponse>> handler)
{
    [Function("ListProducts")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<ProductResponse>> result = await handler.Handle(new ListProductsQuery(), cancellationToken);

        return result.IsSuccess ? new OkObjectResult(result.Value) : result.Error.ToProblem();
    }
}
