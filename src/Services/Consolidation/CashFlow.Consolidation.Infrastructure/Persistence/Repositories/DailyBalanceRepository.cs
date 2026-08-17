using CashFlow.Consolidation.Domain;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Consolidation.Infrastructure.Persistence.Repositories;

public class DailyBalanceRepository : IDailyBalanceRepository
{
    private readonly ConsolidationDbContext _context;

    public DailyBalanceRepository(ConsolidationDbContext context)
    {
        _context = context;
    }

    public async Task<DailyBalance?> GetByMerchantAndDateAsync(
        Guid merchantId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _context.DailyBalances
            .FirstOrDefaultAsync(b => b.MerchantId == merchantId && b.Date == date, cancellationToken);
    }

    public async Task<IReadOnlyList<DailyBalance>> GetByMerchantAndDateRangeAsync(
        Guid merchantId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.DailyBalances
            .AsNoTracking()
            .Where(b => b.MerchantId == merchantId && b.Date >= startDate && b.Date <= endDate)
            .OrderBy(b => b.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DailyBalance balance, CancellationToken cancellationToken = default)
    {
        await _context.DailyBalances.AddAsync(balance, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
