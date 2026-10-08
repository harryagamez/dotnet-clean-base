using CleanBase.Domain.Products;
using FluentValidation;

namespace CleanBase.Application.Products.Create;

/// <summary>
/// Validates the shape of the input and reports every invalid field at once.
/// The <see cref="Product"/> entity still guards its own invariants: this is the first line of defense, not the only one.
/// </summary>
internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(command => command.Sku).NotEmpty().MaximumLength(Product.SkuMaxLength);
        RuleFor(command => command.Price).GreaterThanOrEqualTo(0);
    }
}
