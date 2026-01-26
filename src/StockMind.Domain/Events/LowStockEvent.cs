using StockMind.Domain.Common;

namespace StockMind.Domain.Events;

public sealed class LowStockEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int CurrentQuantity { get; }
    public int MinimumQuantity { get; }
    public DateTime OccurredOn { get; }

    public LowStockEvent(Guid productId, string productName, int currentQuantity, int minimumQuantity)
    {
        ProductId = productId;
        ProductName = productName;
        CurrentQuantity = currentQuantity;
        MinimumQuantity = minimumQuantity;
        OccurredOn = DateTime.UtcNow;
    }
}
