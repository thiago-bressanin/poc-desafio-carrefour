namespace CashFlow.Consolidation.Domain;

public interface IProcessedTransactionRepository
{
    Task<bool> HasBeenProcessedAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
