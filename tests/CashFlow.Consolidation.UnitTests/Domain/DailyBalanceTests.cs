using CashFlow.BuildingBlocks.Domain;
using CashFlow.Consolidation.Domain;
using FluentAssertions;
using Xunit;

namespace CashFlow.Consolidation.UnitTests.Domain;

public class DailyBalanceTests
{
    private readonly Guid _merchantId = Guid.NewGuid();
    private readonly DateOnly _date = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Constructor_ShouldInitializeWithZeroAmounts()
    {
        // Act
        var dailyBalance = new DailyBalance(_merchantId, _date);

        // Assert
        dailyBalance.Should().NotBeNull();
        dailyBalance.MerchantId.Should().Be(_merchantId);
        dailyBalance.Date.Should().Be(_date);
        dailyBalance.TotalCredits.Should().Be(0m);
        dailyBalance.TotalDebits.Should().Be(0m);
        dailyBalance.NetBalance.Should().Be(0m);
        dailyBalance.TotalTransactions.Should().Be(0);
    }

    [Fact]
    public void ApplyCredit_ShouldIncrementCreditsAndTransactions()
    {
        // Arrange
        var dailyBalance = new DailyBalance(_merchantId, _date);

        // Act
        dailyBalance.ApplyCredit(100.50m);
        dailyBalance.ApplyCredit(49.50m);

        // Assert
        dailyBalance.TotalCredits.Should().Be(150.00m);
        dailyBalance.TotalDebits.Should().Be(0m);
        dailyBalance.NetBalance.Should().Be(150.00m);
        dailyBalance.TotalTransactions.Should().Be(2);
    }

    [Fact]
    public void ApplyDebit_ShouldIncrementDebitsAndTransactions()
    {
        // Arrange
        var dailyBalance = new DailyBalance(_merchantId, _date);

        // Act
        dailyBalance.ApplyDebit(75.25m);

        // Assert
        dailyBalance.TotalCredits.Should().Be(0m);
        dailyBalance.TotalDebits.Should().Be(75.25m);
        dailyBalance.NetBalance.Should().Be(-75.25m);
        dailyBalance.TotalTransactions.Should().Be(1);
    }

    [Fact]
    public void ApplyCreditsAndDebits_ShouldAccuratelyCalculateNetBalance()
    {
        // Arrange
        var dailyBalance = new DailyBalance(_merchantId, _date);

        // Act
        dailyBalance.ApplyCredit(500.00m);
        dailyBalance.ApplyDebit(150.00m);
        dailyBalance.ApplyDebit(50.00m);

        // Assert
        dailyBalance.TotalCredits.Should().Be(500.00m);
        dailyBalance.TotalDebits.Should().Be(200.00m);
        dailyBalance.NetBalance.Should().Be(300.00m);
        dailyBalance.TotalTransactions.Should().Be(3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void ApplyCredit_WithInvalidAmount_ShouldThrowBusinessRuleException(decimal invalidAmount)
    {
        // Arrange
        var dailyBalance = new DailyBalance(_merchantId, _date);

        // Act
        Action act = () => dailyBalance.ApplyCredit(invalidAmount);

        // Assert
        act.Should().Throw<BusinessRuleException>();
    }
}
