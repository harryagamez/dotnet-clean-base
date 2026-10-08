using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Application.Products.Create;
using CleanBase.Domain.Common;
using CleanBase.FunctionApp.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace CleanBase.FunctionApp.Functions.Products;

public sealed class CreateProductFunction(ICommandHandler<CreateProductCommand, Guid> handler)
{
    [Function("CreateProduct")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "products")] HttpRequest request,
        [FromBody] CreateProductRequest? body,
        CancellationToken cancellationToken)
    {
        if (body is null)
        {
            return ProblemResults.MissingBody();
        }

        Result<Guid> result = await handler.Handle(
            new CreateProductCommand(body.Name, body.Sku, body.Price),
            cancellationToken);

        return result.IsSuccess
            ? new CreatedResult($"{request.Path}/{result.Value}", result.Value)
            : result.Error.ToProblem();
    }
}
