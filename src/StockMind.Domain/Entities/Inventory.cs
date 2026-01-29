using StockMind.Domain.Common;
using StockMind.Domain.Enums;

namespace StockMind.Domain.Entities;

/// <summary>
/// Inventário Físico
/// Processo de contagem física para ajuste de estoque
/// </summary>
public sealed class Inventory : AggregateRoot
{
    public Guid WarehouseId { get; private set; }
    public string InventoryNumber { get; private set; }
    public InventoryStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public Guid StartedByUserId { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? Notes { get; private set; }
    
    // Navigation properties
    public Warehouse Warehouse { get; private set; } = null!;
    public ICollection<InventoryItem> Items { get; private set; } = new List<InventoryItem>();
    public ICollection<StockMovement> StockMovements { get; private set; } = new List<StockMovement>();

    // EF Core constructor
    private Inventory()
    {
        InventoryNumber = string.Empty;
    }

    private Inventory(Guid warehouseId, string inventoryNumber, Guid startedByUserId, string? notes)
    {
        WarehouseId = warehouseId;
        InventoryNumber = inventoryNumber;
        StartedByUserId = startedByUserId;
        Notes = notes;
        Status = InventoryStatus.InProgress;
        StartDate = DateTime.UtcNow;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static Inventory Create(Guid warehouseId, string inventoryNumber, Guid startedByUserId, string? notes = null)
    {
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse ID cannot be empty", nameof(warehouseId));

        if (string.IsNullOrWhiteSpace(inventoryNumber))
            throw new ArgumentException("Inventory number cannot be empty", nameof(inventoryNumber));

        if (inventoryNumber.Length > 50)
            throw new ArgumentException("Inventory number cannot exceed 50 characters", nameof(inventoryNumber));

        if (startedByUserId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(startedByUserId));

        return new Inventory(warehouseId, inventoryNumber.Trim(), startedByUserId, notes?.Trim());
    }

    public void AddItem(Guid productId, decimal systemQuantity, decimal physicalQuantity)
    {
        if (Status != InventoryStatus.InProgress)
            throw new InvalidOperationException("Can only add items to in-progress inventory");

        // Verifica se já existe item para este produto
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
            throw new InvalidOperationException("Product already added to inventory");

        var item = InventoryItem.Create(Id, productId, systemQuantity, physicalQuantity);
        Items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateItemPhysicalQuantity(Guid itemId, decimal physicalQuantity)
    {
        if (Status != InventoryStatus.InProgress)
            throw new InvalidOperationException("Can only update items in in-progress inventory");

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException("Item not found");

        item.UpdatePhysicalQuantity(physicalQuantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SubmitForApproval()
    {
        if (Status != InventoryStatus.InProgress)
            throw new InvalidOperationException("Only in-progress inventory can be submitted");

        if (!Items.Any())
            throw new InvalidOperationException("Cannot submit empty inventory");

        Status = InventoryStatus.PendingApproval;
        EndDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(Guid approvedByUserId)
    {
        if (Status != InventoryStatus.PendingApproval)
            throw new InvalidOperationException("Only pending inventory can be approved");

        if (approvedByUserId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(approvedByUserId));

        Status = InventoryStatus.Completed;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == InventoryStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed inventory");

        Status = InventoryStatus.Cancelled;
        EndDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public int GetTotalItemsCount() => Items.Count;
    
    public int GetItemsWithDifferenceCount() => Items.Count(i => i.HasDifference());
    
    public decimal GetTotalDifferenceValue() => Items.Sum(i => i.Difference);
}

/// <summary>
/// Item do Inventário
/// </summary>
public sealed class InventoryItem : BaseEntity
{
    public Guid InventoryId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal SystemQuantity { get; private set; }
    public decimal PhysicalQuantity { get; private set; }
    public decimal Difference { get; private set; }
    public string? Notes { get; private set; }
    
    // Navigation properties
    public Inventory Inventory { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    // EF Core constructor
    private InventoryItem()
    {
    }

    private InventoryItem(Guid inventoryId, Guid productId, decimal systemQuantity, decimal physicalQuantity)
    {
        InventoryId = inventoryId;
        ProductId = productId;
        SystemQuantity = systemQuantity;
        PhysicalQuantity = physicalQuantity;
        Difference = physicalQuantity - systemQuantity;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static InventoryItem Create(Guid inventoryId, Guid productId, decimal systemQuantity, decimal physicalQuantity)
    {
        if (inventoryId == Guid.Empty)
            throw new ArgumentException("Inventory ID cannot be empty", nameof(inventoryId));

        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (systemQuantity < 0)
            throw new ArgumentException("System quantity cannot be negative", nameof(systemQuantity));

        if (physicalQuantity < 0)
            throw new ArgumentException("Physical quantity cannot be negative", nameof(physicalQuantity));

        return new InventoryItem(inventoryId, productId, systemQuantity, physicalQuantity);
    }

    public void UpdatePhysicalQuantity(decimal physicalQuantity)
    {
        if (physicalQuantity < 0)
            throw new ArgumentException("Physical quantity cannot be negative", nameof(physicalQuantity));

        PhysicalQuantity = physicalQuantity;
        Difference = physicalQuantity - SystemQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSystemQuantity(decimal systemQuantity)
    {
        if (systemQuantity < 0)
            throw new ArgumentException("System quantity cannot be negative", nameof(systemQuantity));

        SystemQuantity = systemQuantity;
        Difference = PhysicalQuantity - systemQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasDifference() => Difference != 0;
    
    public bool HasSurplus() => Difference > 0;
    
    public bool HasShortage() => Difference < 0;
}
