using CleanBase.Application.Abstractions.Data;
using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Domain.Common;
using CleanBase.Domain.Products;

namespace CleanBase.Application.Products.Delete;

internal sealed class DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.Id));
        }

        productRepository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
