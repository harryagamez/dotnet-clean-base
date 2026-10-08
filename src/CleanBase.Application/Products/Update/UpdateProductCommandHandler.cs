using CleanBase.Application.Abstractions.Data;
using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Domain.Common;
using CleanBase.Domain.Products;

namespace CleanBase.Application.Products.Update;

internal sealed class UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.Id));
        }

        Result updateResult = product.Update(command.Name, command.Price);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
