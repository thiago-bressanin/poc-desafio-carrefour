namespace CashFlow.Consolidation.Domain;

public interface IDailyBalanceRepository
{
    Task<DailyBalance?> GetByMerchantAndDateAsync(
        Guid merchantId, 
        DateOnly date, 
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyBalance>> GetByMerchantAndDateRangeAsync(
        Guid merchantId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken cancellationToken = default);

    Task AddAsync(DailyBalance balance, CancellationToken cancellationToken = default);
    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
