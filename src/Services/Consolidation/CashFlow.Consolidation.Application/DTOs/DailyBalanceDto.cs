namespace CashFlow.Consolidation.Application.DTOs;

public record DailyBalanceDto
{
    public Guid MerchantId { get; init; }
    public DateOnly Date { get; init; }
    public decimal TotalCredits { get; init; }
    public decimal TotalDebits { get; init; }
    public decimal NetBalance { get; init; }
    public decimal CumulativeBalance { get; init; }
    public int TotalTransactions { get; init; }
    public DateTime LastUpdatedAt { get; init; }
}
