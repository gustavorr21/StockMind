using StockMind.Domain.Common;

namespace StockMind.Domain.Events;

public sealed record LowStockDetectedEvent : IDomainEvent
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSku { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public decimal CurrentQuantity { get; init; }
    public decimal MinimumQuantity { get; init; }
    public decimal AvailableQuantity { get; init; }
    public DateTime DetectedAt { get; init; }
    public DateTime OccurredOn { get; init; }

    public LowStockDetectedEvent(
        Guid productId,
        string productName,
        string productSku,
        Guid warehouseId,
        string warehouseName,
        decimal currentQuantity,
        decimal minimumQuantity,
        decimal availableQuantity)
    {
        ProductId = productId;
        ProductName = productName;
        ProductSku = productSku;
        WarehouseId = warehouseId;
        WarehouseName = warehouseName;
        CurrentQuantity = currentQuantity;
        MinimumQuantity = minimumQuantity;
        AvailableQuantity = availableQuantity;
        DetectedAt = DateTime.UtcNow;
        OccurredOn = DateTime.UtcNow;
    }
}
