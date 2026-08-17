using CashFlow.Consolidation.Application.DTOs;

namespace CashFlow.Consolidation.Application.Interfaces;

public interface IConsolidationCacheService
{
    Task<DailyBalanceDto?> GetDailyBalanceAsync(Guid merchantId, DateOnly date, CancellationToken cancellationToken = default);
    Task SetDailyBalanceAsync(Guid merchantId, DateOnly date, DailyBalanceDto dto, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task InvalidateDailyBalanceAsync(Guid merchantId, DateOnly date, CancellationToken cancellationToken = default);
}
