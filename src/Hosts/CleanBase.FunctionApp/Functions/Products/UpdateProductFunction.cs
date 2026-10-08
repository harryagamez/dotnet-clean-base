using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Application.Products.Update;
using CleanBase.Domain.Common;
using CleanBase.FunctionApp.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace CleanBase.FunctionApp.Functions.Products;

public sealed class UpdateProductFunction(ICommandHandler<UpdateProductCommand> handler)
{
    [Function("UpdateProduct")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "products/{id:guid}")] HttpRequest request,
        Guid id,
        [FromBody] UpdateProductRequest? body,
        CancellationToken cancellationToken)
    {
        if (body is null)
        {
            return ProblemResults.MissingBody();
        }

        Result result = await handler.Handle(new UpdateProductCommand(id, body.Name, body.Price), cancellationToken);

        return result.IsSuccess ? new NoContentResult() : result.Error.ToProblem();
    }
}
