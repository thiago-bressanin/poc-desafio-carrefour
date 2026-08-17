namespace CashFlow.Transactions.Application.DTOs;

public record TransactionResponseDto
{
    public Guid Id { get; init; }
    public Guid MerchantId { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime Date { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
