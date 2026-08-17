namespace CashFlow.BuildingBlocks.Domain;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
