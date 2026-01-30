using MediatR;

namespace StockMind.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}

