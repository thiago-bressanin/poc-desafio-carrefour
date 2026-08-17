namespace CashFlow.Transactions.Domain;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetByMerchantAndDateRangeAsync(
        Guid merchantId, 
        DateTime startDate, 
        DateTime endDate, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default);
    Task<int> CountByMerchantAndDateRangeAsync(
        Guid merchantId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
