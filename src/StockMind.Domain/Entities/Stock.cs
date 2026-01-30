using StockMind.Domain.Common;

namespace StockMind.Domain.Entities;

/// <summary>
/// Entidade Estoque - Tabela de CACHE/PERFORMANCE
/// Representa a posição atual de estoque de um produto em um depósito
/// IMPORTANTE: Esta tabela é derivada das movimentações, não a fonte da verdade
/// </summary>
public sealed class Stock : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal CurrentQuantity { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public decimal AvailableQuantity { get; private set; } // CurrentQuantity - ReservedQuantity
    public DateTime LastMovementDate { get; private set; }
    
    // Navigation properties
    public Product Product { get; private set; } = null!;
    public Warehouse Warehouse { get; private set; } = null!;

    // EF Core constructor
    private Stock()
    {
    }

    private Stock(Guid productId, Guid warehouseId)
    {
        ProductId = productId;
        WarehouseId = warehouseId;
        CurrentQuantity = 0;
        ReservedQuantity = 0;
        AvailableQuantity = 0;
        LastMovementDate = DateTime.UtcNow;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public static Stock Create(Guid productId, Guid warehouseId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse ID cannot be empty", nameof(warehouseId));

        return new Stock(productId, warehouseId);
    }

    /// <summary>
    /// Adiciona quantidade ao estoque (entrada)
    /// </summary>
    public void AddQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        CurrentQuantity += quantity;
        RecalculateAvailableQuantity();
        LastMovementDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove quantidade do estoque (saída)
    /// </summary>
    public void RemoveQuantity(decimal quantity, bool allowNegative = false)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (!allowNegative && (CurrentQuantity - quantity) < 0)
            throw new InvalidOperationException($"Insufficient stock. Available: {AvailableQuantity}, Requested: {quantity}");

        CurrentQuantity -= quantity;
        RecalculateAvailableQuantity();
        LastMovementDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ajusta a quantidade para um valor específico (usado em inventários)
    /// </summary>
    public void AdjustQuantity(decimal newQuantity)
    {
        if (newQuantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(newQuantity));

        CurrentQuantity = newQuantity;
        RecalculateAvailableQuantity();
        LastMovementDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reserva quantidade (para pedidos, etc)
    /// </summary>
    public void ReserveQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if ((ReservedQuantity + quantity) > CurrentQuantity)
            throw new InvalidOperationException($"Cannot reserve {quantity}. Available: {AvailableQuantity}");

        ReservedQuantity += quantity;
        RecalculateAvailableQuantity();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Libera quantidade reservada
    /// </summary>
    public void ReleaseReservedQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException($"Cannot release {quantity}. Reserved: {ReservedQuantity}");

        ReservedQuantity -= quantity;
        RecalculateAvailableQuantity();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Confirma saída de quantidade reservada
    /// </summary>
    public void ConfirmReservedExit(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException($"Cannot confirm exit of {quantity}. Reserved: {ReservedQuantity}");

        CurrentQuantity -= quantity;
        ReservedQuantity -= quantity;
        RecalculateAvailableQuantity();
        LastMovementDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateAvailableQuantity()
    {
        AvailableQuantity = CurrentQuantity - ReservedQuantity;
    }

    /// <summary>
    /// Verifica se há estoque disponível suficiente
    /// </summary>
    public bool HasAvailableQuantity(decimal quantity)
    {
        return AvailableQuantity >= quantity;
    }

    /// <summary>
    /// Verifica se está abaixo do estoque mínimo
    /// </summary>
    public bool IsBelowMinimum(int minimumStock)
    {
        return CurrentQuantity < minimumStock;
    }

    /// <summary>
    /// Verifica se excede o estoque máximo
    /// </summary>
    public bool IsAboveMaximum(int maximumStock)
    {
        return maximumStock > 0 && CurrentQuantity > maximumStock;
    }
}
