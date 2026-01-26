using StockMind.Domain.Common;

namespace StockMind.Domain.Events;

public sealed class ProductCreatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public string Sku { get; }
    public DateTime OccurredOn { get; }

    public ProductCreatedEvent(Guid productId, string productName, string sku)
    {
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
        OccurredOn = DateTime.UtcNow;
    }
}
