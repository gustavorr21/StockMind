namespace StockMind.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
