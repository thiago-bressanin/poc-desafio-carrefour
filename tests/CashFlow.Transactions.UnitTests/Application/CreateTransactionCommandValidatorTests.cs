using CashFlow.Transactions.Application.Commands.CreateTransaction;
using FluentAssertions;
using Xunit;

namespace CashFlow.Transactions.UnitTests.Application;

public class CreateTransactionCommandValidatorTests
{
    private readonly CreateTransactionCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = new CreateTransactionCommand(
            Guid.NewGuid(),
            "Credit",
            100m,
            DateTime.UtcNow,
            "Depósito"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("")]
    [InlineData("Transfer")]
    public void Validate_WithInvalidType_ShouldHaveValidationError(string invalidType)
    {
        // Arrange
        var command = new CreateTransactionCommand(
            Guid.NewGuid(),
            invalidType,
            100m,
            DateTime.UtcNow,
            "Descrição"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Type");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_WithInvalidAmount_ShouldHaveValidationError(decimal invalidAmount)
    {
        // Arrange
        var command = new CreateTransactionCommand(
            Guid.NewGuid(),
            "Debit",
            invalidAmount,
            DateTime.UtcNow,
            "Pagamento"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
    }
}
