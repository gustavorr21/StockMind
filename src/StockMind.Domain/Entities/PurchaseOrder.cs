using StockMind.Domain.Common;
using StockMind.Domain.Enums;

namespace StockMind.Domain.Entities;

/// <summary>
/// Pedido de Compra
/// </summary>
public sealed class PurchaseOrder : AggregateRoot
{
    public Guid SupplierId { get; private set; }
    public string OrderNumber { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public DateTime? ActualDeliveryDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Notes { get; private set; }
    
    // Navigation properties
    public Supplier Supplier { get; private set; } = null!;
    public ICollection<PurchaseOrderItem> Items { get; private set; } = new List<PurchaseOrderItem>();
    public ICollection<PurchaseEntry> PurchaseEntries { get; private set; } = new List<PurchaseEntry>();

    // EF Core constructor
    private PurchaseOrder()
    {
        OrderNumber = string.Empty;
    }

    private PurchaseOrder(Guid supplierId, string orderNumber, DateTime? expectedDeliveryDate, string? notes)
    {
        SupplierId = supplierId;
        OrderNumber = orderNumber;
        Status = PurchaseOrderStatus.Pending;
        OrderDate = DateTime.UtcNow;
        ExpectedDeliveryDate = expectedDeliveryDate;
        Notes = notes;
        TotalAmount = 0;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static PurchaseOrder Create(Guid supplierId, string orderNumber, DateTime? expectedDeliveryDate = null, string? notes = null)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID cannot be empty", nameof(supplierId));

        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number cannot be empty", nameof(orderNumber));

        if (orderNumber.Length > 50)
            throw new ArgumentException("Order number cannot exceed 50 characters", nameof(orderNumber));

        return new PurchaseOrder(supplierId, orderNumber.Trim(), expectedDeliveryDate, notes?.Trim());
    }

    public void AddItem(Guid productId, decimal quantity, decimal unitCost)
    {
        if (Status != PurchaseOrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to a non-pending purchase order");

        var item = PurchaseOrderItem.Create(Id, productId, quantity, unitCost);
        Items.Add(item);
        RecalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status != PurchaseOrderStatus.Pending)
            throw new InvalidOperationException("Cannot remove items from a non-pending purchase order");

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
            RecalculateTotalAmount();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Confirm()
    {
        if (Status != PurchaseOrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        if (!Items.Any())
            throw new InvalidOperationException("Cannot confirm order without items");

        Status = PurchaseOrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsInTransit()
    {
        if (Status != PurchaseOrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be marked as in transit");

        Status = PurchaseOrderStatus.InTransit;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPartiallyReceived()
    {
        if (Status != PurchaseOrderStatus.Confirmed && Status != PurchaseOrderStatus.InTransit)
            throw new InvalidOperationException("Invalid status transition");

        Status = PurchaseOrderStatus.PartiallyReceived;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsReceived(DateTime actualDeliveryDate)
    {
        if (Status == PurchaseOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot mark cancelled order as received");

        Status = PurchaseOrderStatus.Received;
        ActualDeliveryDate = actualDeliveryDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == PurchaseOrderStatus.Received)
            throw new InvalidOperationException("Cannot cancel received order");

        Status = PurchaseOrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateExpectedDeliveryDate(DateTime expectedDeliveryDate)
    {
        ExpectedDeliveryDate = expectedDeliveryDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotalAmount()
    {
        TotalAmount = Items.Sum(i => i.TotalCost);
    }

    public bool CanBeModified() => Status == PurchaseOrderStatus.Pending;
    
    public bool IsFullyReceived() => Status == PurchaseOrderStatus.Received;
}
