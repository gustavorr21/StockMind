using StockMind.Domain.Common;

namespace StockMind.Domain.Entities;

/// <summary>
/// Entidade Depósito/Almoxarifado
/// Representa um local físico onde produtos são armazenados
/// </summary>
public sealed class Warehouse : BaseEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsMain { get; private set; } // Depósito principal
    
    // Navigation properties
    public ICollection<Stock> Stocks { get; private set; } = new List<Stock>();
    public ICollection<StockMovement> StockMovements { get; private set; } = new List<StockMovement>();
    public ICollection<Inventory> Inventories { get; private set; } = new List<Inventory>();

    // EF Core constructor
    private Warehouse()
    {
        Name = string.Empty;
    }

    private Warehouse(string name, string? description, string? address, bool isMain)
    {
        Name = name;
        Description = description;
        Address = address;
        IsMain = isMain;
        IsActive = true;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static Warehouse Create(string name, string? description = null, string? address = null, bool isMain = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Warehouse name cannot exceed 200 characters", nameof(name));

        return new Warehouse(name.Trim(), description?.Trim(), address?.Trim(), isMain);
    }

    public void UpdateBasicInfo(string name, string? description, string? address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Warehouse name cannot exceed 200 characters", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        Address = address?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsMain()
    {
        IsMain = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveAsMain()
    {
        IsMain = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
