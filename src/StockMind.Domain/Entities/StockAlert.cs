using StockMind.Domain.Common;
using StockMind.Domain.Enums;

namespace StockMind.Domain.Entities;

public sealed class StockAlert : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string ProductSku { get; private set; } = string.Empty;
    public Guid WarehouseId { get; private set; }
    public string WarehouseName { get; private set; } = string.Empty;
    public decimal CurrentQuantity { get; private set; }
    public decimal MinimumQuantity { get; private set; }
    public AlertStatus Status { get; private set; }
    public DateTime FirstDetectedAt { get; private set; }
    public DateTime LastNotifiedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public Guid? AcknowledgedByUserId { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public Product Product { get; private set; } = null!;
    public Warehouse Warehouse { get; private set; } = null!;

    // EF Core constructor
    private StockAlert() { }

    private StockAlert(
        Guid productId,
        string productName,
        string productSku,
        Guid warehouseId,
        string warehouseName,
        decimal currentQuantity,
        decimal minimumQuantity)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        ProductSku = productSku;
        WarehouseId = warehouseId;
        WarehouseName = warehouseName;
        CurrentQuantity = currentQuantity;
        MinimumQuantity = minimumQuantity;
        Status = AlertStatus.Active;
        FirstDetectedAt = DateTime.UtcNow;
        LastNotifiedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public static StockAlert Create(
        Guid productId,
        string productName,
        string productSku,
        Guid warehouseId,
        string warehouseName,
        decimal currentQuantity,
        decimal minimumQuantity)
    {
        if (currentQuantity < 0)
            throw new ArgumentException("Current quantity cannot be negative", nameof(currentQuantity));

        if (minimumQuantity < 0)
            throw new ArgumentException("Minimum quantity cannot be negative", nameof(minimumQuantity));

        return new StockAlert(
            productId,
            productName,
            productSku,
            warehouseId,
            warehouseName,
            currentQuantity,
            minimumQuantity);
    }

    public void UpdateQuantity(decimal newQuantity)
    {
        CurrentQuantity = newQuantity;
        LastNotifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resolve()
    {
        if (Status == AlertStatus.Resolved)
            return;

        Status = AlertStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Acknowledge(Guid userId, string? notes = null)
    {
        if (Status == AlertStatus.Resolved)
            throw new InvalidOperationException("Cannot acknowledge a resolved alert");

        Status = AlertStatus.Acknowledged;
        AcknowledgedAt = DateTime.UtcNow;
        AcknowledgedByUserId = userId;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive() => Status == AlertStatus.Active;
    public bool ShouldSendEmail() => Status == AlertStatus.Active;
}
