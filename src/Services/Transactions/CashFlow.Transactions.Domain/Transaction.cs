using CashFlow.BuildingBlocks.Domain;

namespace CashFlow.Transactions.Domain;

public class Transaction : AggregateRoot<Guid>
{
    public Guid MerchantId { get; private set; }
    public TransactionType Type { get; private set; }
    public Money Money { get; private set; } = null!;
    public decimal Amount => Money.Amount;
    public DateTime Date { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    // Required by EF Core
    protected Transaction() : base() { }

    private Transaction(
        Guid id,
        Guid merchantId,
        TransactionType type,
        Money money,
        DateTime date,
        string description,
        DateTime createdAt) : base(id)
    {
        if (merchantId == Guid.Empty)
            throw new BusinessRuleException("O identificador do lojista (MerchantId) é obrigatório.", "EMPTY_MERCHANT_ID");

        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleException("A descrição do lançamento é obrigatória.", "EMPTY_DESCRIPTION");

        if (description.Length > 250)
            throw new BusinessRuleException("A descrição não pode exceder 250 caracteres.", "DESCRIPTION_TOO_LONG");

        if (date == default)
            throw new BusinessRuleException("A data do lançamento é obrigatória e deve ser válida.", "INVALID_DATE");

        MerchantId = merchantId;
        Type = type;
        Money = money ?? throw new ArgumentNullException(nameof(money));
        Date = date.Date; // Normalize to date component or preserve as needed
        Description = description.Trim();
        CreatedAt = createdAt;
    }

    public static Transaction Create(
        Guid merchantId,
        TransactionType type,
        decimal amount,
        DateTime date,
        string description)
    {
        var id = Guid.NewGuid();
        var money = Money.Create(amount);
        var createdAt = DateTime.UtcNow;

        return new Transaction(id, merchantId, type, money, date, description, createdAt);
    }

    public static Transaction CreateCredit(
        Guid merchantId,
        decimal amount,
        DateTime date,
        string description)
    {
        return Create(merchantId, TransactionType.Credit, amount, date, description);
    }

    public static Transaction CreateDebit(
        Guid merchantId,
        decimal amount,
        DateTime date,
        string description)
    {
        return Create(merchantId, TransactionType.Debit, amount, date, description);
    }
}
