using CleanBase.Domain.Products;
using FluentValidation;

namespace CleanBase.Application.Products.Update;

internal sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(command => command.Price).GreaterThanOrEqualTo(0);
    }
}
