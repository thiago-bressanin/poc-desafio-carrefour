namespace CashFlow.BuildingBlocks.Messaging;

public record TransactionCreatedIntegrationEvent
{
    public Guid TransactionId { get; init; }
    public Guid MerchantId { get; init; }
    public string Type { get; init; } = string.Empty; // "Credit" | "Debit"
    public decimal Amount { get; init; }
    public DateTime Date { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
