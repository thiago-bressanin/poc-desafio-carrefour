namespace CashFlow.Consolidation.Application.DTOs;

public record DailyBalanceReportDto
{
    public Guid MerchantId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal TotalPeriodCredits { get; init; }
    public decimal TotalPeriodDebits { get; init; }
    public decimal TotalPeriodNetBalance { get; init; }
    public IReadOnlyList<DailyBalanceDto> DailyBalances { get; init; } = Array.Empty<DailyBalanceDto>();
}
