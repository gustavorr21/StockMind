using StockMind.Domain.Common;

namespace StockMind.Domain.Entities;

/// <summary>
/// Item do Pedido de Compra
/// </summary>
public sealed class PurchaseOrderItem : BaseEntity
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    
    // Navigation properties
    public PurchaseOrder PurchaseOrder { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    // EF Core constructor
    private PurchaseOrderItem()
    {
    }

    private PurchaseOrderItem(Guid purchaseOrderId, Guid productId, decimal quantity, decimal unitCost)
    {
        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        Quantity = quantity;
        UnitCost = unitCost;
        TotalCost = quantity * unitCost;
        ReceivedQuantity = 0;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static PurchaseOrderItem Create(Guid purchaseOrderId, Guid productId, decimal quantity, decimal unitCost)
    {
        if (purchaseOrderId == Guid.Empty)
            throw new ArgumentException("Purchase order ID cannot be empty", nameof(purchaseOrderId));

        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (unitCost < 0)
            throw new ArgumentException("Unit cost cannot be negative", nameof(unitCost));

        return new PurchaseOrderItem(purchaseOrderId, productId, quantity, unitCost);
    }

    public void UpdateQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Quantity = quantity;
        TotalCost = quantity * UnitCost;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateUnitCost(decimal unitCost)
    {
        if (unitCost < 0)
            throw new ArgumentException("Unit cost cannot be negative", nameof(unitCost));

        UnitCost = unitCost;
        TotalCost = Quantity * unitCost;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegisterReceivedQuantity(decimal receivedQuantity)
    {
        if (receivedQuantity < 0)
            throw new ArgumentException("Received quantity cannot be negative", nameof(receivedQuantity));

        if (ReceivedQuantity + receivedQuantity > Quantity)
            throw new InvalidOperationException($"Total received quantity cannot exceed ordered quantity ({Quantity})");

        ReceivedQuantity += receivedQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetPendingQuantity() => Quantity - ReceivedQuantity;
    
    public bool IsFullyReceived() => ReceivedQuantity >= Quantity;
    
    public bool IsPartiallyReceived() => ReceivedQuantity > 0 && ReceivedQuantity < Quantity;
}
