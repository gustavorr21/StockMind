using StockMind.Domain.Common;

namespace StockMind.Domain.Entities;

/// <summary>
/// Entrada de Compra (Recebimento)
/// Registra o recebimento físico de um pedido de compra
/// Ao confirmar, gera movimentações de estoque
/// </summary>
public sealed class PurchaseEntry : AggregateRoot
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public DateTime EntryDate { get; private set; }
    public string? InvoiceNumber { get; private set; }
    public string? Notes { get; private set; }
    public Guid ReceivedByUserId { get; private set; }
    public bool IsConfirmed { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    
    // Navigation properties
    public PurchaseOrder PurchaseOrder { get; private set; } = null!;
    public Warehouse Warehouse { get; private set; } = null!;
    public ICollection<PurchaseEntryItem> Items { get; private set; } = new List<PurchaseEntryItem>();
    public ICollection<StockMovement> StockMovements { get; private set; } = new List<StockMovement>();

    // EF Core constructor
    private PurchaseEntry()
    {
    }

    private PurchaseEntry(
        Guid purchaseOrderId,
        Guid warehouseId,
        Guid receivedByUserId,
        string? invoiceNumber,
        string? notes)
    {
        PurchaseOrderId = purchaseOrderId;
        WarehouseId = warehouseId;
        ReceivedByUserId = receivedByUserId;
        InvoiceNumber = invoiceNumber;
        Notes = notes;
        EntryDate = DateTime.UtcNow;
        IsConfirmed = false;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static PurchaseEntry Create(
        Guid purchaseOrderId,
        Guid warehouseId,
        Guid receivedByUserId,
        string? invoiceNumber = null,
        string? notes = null)
    {
        if (purchaseOrderId == Guid.Empty)
            throw new ArgumentException("Purchase order ID cannot be empty", nameof(purchaseOrderId));

        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse ID cannot be empty", nameof(warehouseId));

        if (receivedByUserId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(receivedByUserId));

        return new PurchaseEntry(
            purchaseOrderId,
            warehouseId,
            receivedByUserId,
            invoiceNumber?.Trim(),
            notes?.Trim());
    }

    public void AddItem(Guid productId, decimal receivedQuantity, string? batchNumber = null, DateTime? expirationDate = null)
    {
        if (IsConfirmed)
            throw new InvalidOperationException("Cannot add items to confirmed entry");

        var item = PurchaseEntryItem.Create(Id, productId, receivedQuantity, batchNumber, expirationDate);
        Items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid itemId)
    {
        if (IsConfirmed)
            throw new InvalidOperationException("Cannot remove items from confirmed entry");

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Confirm()
    {
        if (IsConfirmed)
            throw new InvalidOperationException("Entry already confirmed");

        if (!Items.Any())
            throw new InvalidOperationException("Cannot confirm entry without items");

        IsConfirmed = true;
        ConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInvoiceNumber(string invoiceNumber)
    {
        if (IsConfirmed)
            throw new InvalidOperationException("Cannot update confirmed entry");

        InvoiceNumber = invoiceNumber?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Item da Entrada de Compra
/// </summary>
public sealed class PurchaseEntryItem : BaseEntity
{
    public Guid PurchaseEntryId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public string? BatchNumber { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    
    // Navigation properties
    public PurchaseEntry PurchaseEntry { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    // EF Core constructor
    private PurchaseEntryItem()
    {
    }

    private PurchaseEntryItem(
        Guid purchaseEntryId,
        Guid productId,
        decimal receivedQuantity,
        string? batchNumber,
        DateTime? expirationDate)
    {
        PurchaseEntryId = purchaseEntryId;
        ProductId = productId;
        ReceivedQuantity = receivedQuantity;
        BatchNumber = batchNumber;
        ExpirationDate = expirationDate;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static PurchaseEntryItem Create(
        Guid purchaseEntryId,
        Guid productId,
        decimal receivedQuantity,
        string? batchNumber = null,
        DateTime? expirationDate = null)
    {
        if (purchaseEntryId == Guid.Empty)
            throw new ArgumentException("Purchase entry ID cannot be empty", nameof(purchaseEntryId));

        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (receivedQuantity <= 0)
            throw new ArgumentException("Received quantity must be positive", nameof(receivedQuantity));

        return new PurchaseEntryItem(
            purchaseEntryId,
            productId,
            receivedQuantity,
            batchNumber?.Trim(),
            expirationDate);
    }

    public void UpdateReceivedQuantity(decimal receivedQuantity)
    {
        if (receivedQuantity <= 0)
            throw new ArgumentException("Received quantity must be positive", nameof(receivedQuantity));

        ReceivedQuantity = receivedQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
