using CashFlow.BuildingBlocks.Domain;

namespace CashFlow.Transactions.Domain;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency = "BRL")
    {
        if (amount <= 0)
            throw new BusinessRuleException("O valor do lançamento financeiro deve ser estritamente maior que zero.", "INVALID_AMOUNT");

        if (string.IsNullOrWhiteSpace(currency))
            throw new BusinessRuleException("A moeda deve ser especificada.", "INVALID_CURRENCY");

        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.Trim().ToUpperInvariant();
    }

    public static Money Create(decimal amount, string currency = "BRL")
    {
        return new Money(amount, currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:N2}";
}
