using CashFlow.Transactions.Domain;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Transactions.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionsDbContext _context;

    public TransactionRepository(TransactionsDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetByMerchantAndDateRangeAsync(
        Guid merchantId,
        DateTime startDate,
        DateTime endDate,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var start = startDate.Date;
        var end = endDate.Date.AddDays(1).AddTicks(-1);

        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.MerchantId == merchantId && t.Date >= start && t.Date <= end)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByMerchantAndDateRangeAsync(
        Guid merchantId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var start = startDate.Date;
        var end = endDate.Date.AddDays(1).AddTicks(-1);

        return await _context.Transactions
            .CountAsync(t => t.MerchantId == merchantId && t.Date >= start && t.Date <= end, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
