using StockMind.Domain.Common;

namespace StockMind.Domain.Events;

public sealed class StockUpdatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public int PreviousQuantity { get; }
    public int NewQuantity { get; }
    public string MovementType { get; }
    public DateTime OccurredOn { get; }

    public StockUpdatedEvent(Guid productId, int previousQuantity, int newQuantity, string movementType)
    {
        ProductId = productId;
        PreviousQuantity = previousQuantity;
        NewQuantity = newQuantity;
        MovementType = movementType;
        OccurredOn = DateTime.UtcNow;
    }
}
