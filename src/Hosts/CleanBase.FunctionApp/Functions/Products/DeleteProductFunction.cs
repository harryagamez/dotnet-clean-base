using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Application.Products.Delete;
using CleanBase.Domain.Common;
using CleanBase.FunctionApp.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CleanBase.FunctionApp.Functions.Products;

public sealed class DeleteProductFunction(ICommandHandler<DeleteProductCommand> handler)
{
    [Function("DeleteProduct")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "products/{id:guid}")] HttpRequest request,
        Guid id,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeleteProductCommand(id), cancellationToken);

        return result.IsSuccess ? new NoContentResult() : result.Error.ToProblem();
    }
}
