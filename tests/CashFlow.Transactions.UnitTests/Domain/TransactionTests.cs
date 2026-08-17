using CashFlow.BuildingBlocks.Domain;
using CashFlow.Transactions.Domain;
using FluentAssertions;
using Xunit;

namespace CashFlow.Transactions.UnitTests.Domain;

public class TransactionTests
{
    private readonly Guid _merchantId = Guid.NewGuid();

    [Fact]
    public void CreateCredit_WithValidParameters_ShouldCreateCreditTransaction()
    {
        // Arrange
        var amount = 250.75m;
        var date = DateTime.UtcNow.Date;
        var description = "Venda de mercadorias no PDV 01";

        // Act
        var transaction = Transaction.CreateCredit(_merchantId, amount, date, description);

        // Assert
        transaction.Should().NotBeNull();
        transaction.Id.Should().NotBeEmpty();
        transaction.MerchantId.Should().Be(_merchantId);
        transaction.Type.Should().Be(TransactionType.Credit);
        transaction.Amount.Should().Be(amount);
        transaction.Date.Should().Be(date);
        transaction.Description.Should().Be(description);
        transaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void CreateDebit_WithValidParameters_ShouldCreateDebitTransaction()
    {
        // Arrange
        var amount = 80.00m;
        var date = DateTime.UtcNow.Date;
        var description = "Pagamento de fornecedor de insumos";

        // Act
        var transaction = Transaction.CreateDebit(_merchantId, amount, date, description);

        // Assert
        transaction.Should().NotBeNull();
        transaction.Type.Should().Be(TransactionType.Debit);
        transaction.Amount.Should().Be(amount);
    }

    [Fact]
    public void Create_WithEmptyMerchantId_ShouldThrowBusinessRuleException()
    {
        // Act
        Action act = () => Transaction.CreateCredit(Guid.Empty, 100m, DateTime.UtcNow, "Teste");

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*MerchantId*obrigatório*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyDescription_ShouldThrowBusinessRuleException(string? invalidDescription)
    {
        // Act
        Action act = () => Transaction.CreateCredit(_merchantId, 100m, DateTime.UtcNow, invalidDescription!);

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*descrição*obrigatória*");
    }

    [Fact]
    public void Create_WithDescriptionLongerThan250Chars_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var longDesc = new string('A', 251);

        // Act
        Action act = () => Transaction.CreateCredit(_merchantId, 100m, DateTime.UtcNow, longDesc);

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*250 caracteres*");
    }
}
