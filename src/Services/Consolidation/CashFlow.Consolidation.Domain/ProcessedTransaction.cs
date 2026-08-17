using CashFlow.BuildingBlocks.Domain;

namespace CashFlow.Consolidation.Domain;

public class ProcessedTransaction : Entity<Guid>
{
    public DateTime ProcessedAt { get; private set; }

    // Required by EF Core
    protected ProcessedTransaction() : base() { }

    public ProcessedTransaction(Guid transactionId) : base(transactionId)
    {
        ProcessedAt = DateTime.UtcNow;
    }
}
