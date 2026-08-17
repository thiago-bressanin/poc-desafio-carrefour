namespace CashFlow.Transactions.Application.DTOs;

public record CreateTransactionRequestDto
{
    public Guid MerchantId { get; init; }
    public string Type { get; init; } = string.Empty; // "Credit" | "Debit"
    public decimal Amount { get; init; }
    public DateTime Date { get; init; }
    public string Description { get; init; } = string.Empty;
}
