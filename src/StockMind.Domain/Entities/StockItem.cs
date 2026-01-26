using StockMind.Domain.Common;
using StockMind.Domain.Events;

namespace StockMind.Domain.Entities;

public sealed class StockItem : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => Quantity - ReservedQuantity;
    public string? Location { get; private set; }

    // Navigation property
    public Product? Product { get; private set; }

    private StockItem(Guid productId, int initialQuantity, string? location)
    {
        ProductId = productId;
        Quantity = initialQuantity;
        ReservedQuantity = 0;
        Location = location;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static StockItem Create(Guid productId, int initialQuantity = 0, string? location = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (initialQuantity < 0)
            throw new ArgumentException("Initial quantity cannot be negative", nameof(initialQuantity));

        return new StockItem(productId, initialQuantity, location?.Trim());
    }

    public void AddStock(int quantity, int minimumStock, string productName)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var previousQuantity = Quantity;
        Quantity += quantity;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StockUpdatedEvent(ProductId, previousQuantity, Quantity, "Addition"));
    }

    public void RemoveStock(int quantity, int minimumStock, string productName)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (AvailableQuantity < quantity)
            throw new InvalidOperationException($"Insufficient available stock. Available: {AvailableQuantity}, Requested: {quantity}");

        var previousQuantity = Quantity;
        Quantity -= quantity;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StockUpdatedEvent(ProductId, previousQuantity, Quantity, "Removal"));

        if (Quantity <= minimumStock)
        {
            AddDomainEvent(new LowStockEvent(ProductId, productName, Quantity, minimumStock));
        }
    }

    public void AdjustStock(int newQuantity, int minimumStock, string productName)
    {
        if (newQuantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(newQuantity));

        if (newQuantity < ReservedQuantity)
            throw new InvalidOperationException($"Cannot adjust stock below reserved quantity. Reserved: {ReservedQuantity}");

        var previousQuantity = Quantity;
        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StockUpdatedEvent(ProductId, previousQuantity, Quantity, "Adjustment"));

        if (Quantity <= minimumStock)
        {
            AddDomainEvent(new LowStockEvent(ProductId, productName, Quantity, minimumStock));
        }
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (AvailableQuantity < quantity)
            throw new InvalidOperationException($"Insufficient available stock to reserve. Available: {AvailableQuantity}, Requested: {quantity}");

        ReservedQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservedStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (ReservedQuantity < quantity)
            throw new InvalidOperationException($"Cannot release more than reserved. Reserved: {ReservedQuantity}, Requested: {quantity}");

        ReservedQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLocation(string location)
    {
        Location = location?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
