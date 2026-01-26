using StockMind.Domain.Common;
using StockMind.Domain.Enums;
using StockMind.Domain.Events;
using StockMind.Domain.ValueObjects;

namespace StockMind.Domain.Entities;

public sealed class Product : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Sku { get; private set; }
    public string? Barcode { get; private set; }
    public Money Price { get; private set; }
    public Money CostPrice { get; private set; }
    public ProductStatus Status { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid? SupplierId { get; private set; }
    public int MinimumStock { get; private set; }
    public string? ImageUrl { get; private set; }

    // Navigation properties
    public Category? Category { get; private set; }
    public Supplier? Supplier { get; private set; }

    private Product(string name, string description, string sku, Money price, Money costPrice, Guid categoryId, int minimumStock)
    {
        Name = name;
        Description = description;
        Sku = sku;
        Price = price;
        CostPrice = costPrice;
        CategoryId = categoryId;
        MinimumStock = minimumStock;
        Status = ProductStatus.Active;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductCreatedEvent(Id, Name, Sku));
    }

    public static Product Create(string name, string description, string sku, Money price, Money costPrice, Guid categoryId, int minimumStock = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters", nameof(name));

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));

        if (sku.Length > 50)
            throw new ArgumentException("SKU cannot exceed 50 characters", nameof(sku));

        if (price == null)
            throw new ArgumentNullException(nameof(price));

        if (costPrice == null)
            throw new ArgumentNullException(nameof(costPrice));

        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category ID cannot be empty", nameof(categoryId));

        if (minimumStock < 0)
            throw new ArgumentException("Minimum stock cannot be negative", nameof(minimumStock));

        return new Product(name.Trim(), description?.Trim() ?? string.Empty, sku.Trim().ToUpperInvariant(), price, costPrice, categoryId, minimumStock);
    }

    public void UpdateBasicInfo(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters", nameof(name));

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(Money price)
    {
        Price = price ?? throw new ArgumentNullException(nameof(price));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCostPrice(Money costPrice)
    {
        CostPrice = costPrice ?? throw new ArgumentNullException(nameof(costPrice));
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetBarcode(string barcode)
    {
        if (!string.IsNullOrWhiteSpace(barcode) && barcode.Length > 50)
            throw new ArgumentException("Barcode cannot exceed 50 characters", nameof(barcode));

        Barcode = barcode?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignSupplier(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID cannot be empty", nameof(supplierId));

        SupplierId = supplierId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveSupplier()
    {
        SupplierId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category ID cannot be empty", nameof(categoryId));

        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMinimumStock(int minimumStock)
    {
        if (minimumStock < 0)
            throw new ArgumentException("Minimum stock cannot be negative", nameof(minimumStock));

        MinimumStock = minimumStock;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetImageUrl(string imageUrl)
    {
        ImageUrl = imageUrl?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = ProductStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDiscontinued()
    {
        Status = ProductStatus.Discontinued;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsOutOfStock()
    {
        Status = ProductStatus.OutOfStock;
        UpdatedAt = DateTime.UtcNow;
    }
}
