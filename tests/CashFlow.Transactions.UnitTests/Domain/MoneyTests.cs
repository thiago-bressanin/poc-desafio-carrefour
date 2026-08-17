using CashFlow.BuildingBlocks.Domain;
using CashFlow.Transactions.Domain;
using FluentAssertions;
using Xunit;

namespace CashFlow.Transactions.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldCreateMoneyInstance()
    {
        // Act
        var money = Money.Create(150.50m);

        // Assert
        money.Should().NotBeNull();
        money.Amount.Should().Be(150.50m);
        money.Currency.Should().Be("BRL");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-50)]
    public void Create_WithZeroOrNegativeAmount_ShouldThrowBusinessRuleException(decimal invalidAmount)
    {
        // Act
        Action act = () => Money.Create(invalidAmount);

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public void Equality_WithSameAmountAndCurrency_ShouldBeEqual()
    {
        // Arrange
        var m1 = Money.Create(100m, "BRL");
        var m2 = Money.Create(100.00m, "BRL");

        // Assert
        m1.Should().Be(m2);
        (m1 == m2).Should().BeTrue();
    }
}
