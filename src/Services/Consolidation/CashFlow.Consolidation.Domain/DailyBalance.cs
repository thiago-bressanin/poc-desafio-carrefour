using CashFlow.BuildingBlocks.Domain;

namespace CashFlow.Consolidation.Domain;

public class DailyBalance : AggregateRoot<Guid>
{
    public Guid MerchantId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal TotalCredits { get; private set; }
    public decimal TotalDebits { get; private set; }
    public decimal NetBalance => TotalCredits - TotalDebits;
    public int TotalTransactions { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    // Required by EF Core
    protected DailyBalance() : base() { }

    public DailyBalance(Guid merchantId, DateOnly date) : base(Guid.NewGuid())
    {
        if (merchantId == Guid.Empty)
            throw new BusinessRuleException("O identificador do lojista (MerchantId) é obrigatório.", "EMPTY_MERCHANT_ID");

        MerchantId = merchantId;
        Date = date;
        TotalCredits = 0m;
        TotalDebits = 0m;
        TotalTransactions = 0;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void ApplyCredit(decimal amount)
    {
        if (amount <= 0)
            throw new BusinessRuleException("O valor do crédito deve ser estritamente maior que zero.", "INVALID_AMOUNT");

        TotalCredits = Math.Round(TotalCredits + amount, 2, MidpointRounding.AwayFromZero);
        TotalTransactions++;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void ApplyDebit(decimal amount)
    {
        if (amount <= 0)
            throw new BusinessRuleException("O valor do débito deve ser estritamente maior que zero.", "INVALID_AMOUNT");

        TotalDebits = Math.Round(TotalDebits + amount, 2, MidpointRounding.AwayFromZero);
        TotalTransactions++;
        LastUpdatedAt = DateTime.UtcNow;
    }
}
