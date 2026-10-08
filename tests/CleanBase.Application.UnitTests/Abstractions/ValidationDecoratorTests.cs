using CleanBase.Application.Abstractions.Behaviors;
using CleanBase.Application.Abstractions.Messaging;
using CleanBase.Application.Products.Create;
using CleanBase.Domain.Common;
using FluentValidation;
using NSubstitute;

namespace CleanBase.Application.UnitTests.Abstractions;

public sealed class ValidationDecoratorTests
{
    private readonly ICommandHandler<CreateProductCommand, Guid> _innerHandler =
        Substitute.For<ICommandHandler<CreateProductCommand, Guid>>();

    private readonly ValidationDecorator.CommandHandler<CreateProductCommand, Guid> _decorator;

    public ValidationDecoratorTests()
    {
        IValidator<CreateProductCommand>[] validators = [new CreateProductCommandValidator()];
        _decorator = new ValidationDecorator.CommandHandler<CreateProductCommand, Guid>(_innerHandler, validators);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ReturnsOneErrorPerFieldWithoutCallingHandler()
    {
        Result<Guid> result = await _decorator.Handle(
            new CreateProductCommand(string.Empty, string.Empty, -1m),
            TestContext.Current.CancellationToken);

        ValidationError error = result.Error.ShouldBeOfType<ValidationError>();
        error.Errors.Select(fieldError => fieldError.Code).ShouldBe(
            ["Name", "Sku", "Price"],
            ignoreOrder: true);
        await _innerHandler.DidNotReceive().Handle(Arg.Any<CreateProductCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_DelegatesToHandler()
    {
        var command = new CreateProductCommand("Keyboard", "KB-001", 10m);
        var id = Guid.NewGuid();
        _innerHandler.Handle(command, Arg.Any<CancellationToken>()).Returns(Result.Success(id));

        Result<Guid> result = await _decorator.Handle(command, TestContext.Current.CancellationToken);

        result.Value.ShouldBe(id);
    }
}
