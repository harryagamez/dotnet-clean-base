using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Application.Products;
using CleanBase.Application.Products.GetById;
using CleanBase.Domain.Common;
using CleanBase.FunctionApp.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CleanBase.FunctionApp.Functions.Products;

public sealed class GetProductByIdFunction(IQueryHandler<GetProductByIdQuery, ProductResponse> handler)
{
    [Function("GetProductById")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products/{id:guid}")] HttpRequest request,
        Guid id,
        CancellationToken cancellationToken)
    {
        Result<ProductResponse> result = await handler.Handle(new GetProductByIdQuery(id), cancellationToken);

        return result.IsSuccess ? new OkObjectResult(result.Value) : result.Error.ToProblem();
    }
}
