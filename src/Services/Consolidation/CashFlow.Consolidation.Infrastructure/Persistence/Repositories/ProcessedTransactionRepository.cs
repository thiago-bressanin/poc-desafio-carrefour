using CashFlow.Consolidation.Domain;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Consolidation.Infrastructure.Persistence.Repositories;

public class ProcessedTransactionRepository : IProcessedTransactionRepository
{
    private readonly ConsolidationDbContext _context;

    public ProcessedTransactionRepository(ConsolidationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasBeenProcessedAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.ProcessedTransactions
            .AnyAsync(p => p.Id == transactionId, cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        await _context.ProcessedTransactions.AddAsync(
            new ProcessedTransaction(transactionId), cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
