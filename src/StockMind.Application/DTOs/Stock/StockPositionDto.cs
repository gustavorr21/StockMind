namespace StockMind.Application.DTOs.Stock;

public sealed record StockPositionDto
{
    public Guid StockId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSku { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public decimal CurrentQuantity { get; init; }
    public decimal ReservedQuantity { get; init; }
    public decimal AvailableQuantity { get; init; }
    public int MinimumStock { get; init; }
    public int MaximumStock { get; init; }
    public DateTime LastMovementDate { get; init; }
    public bool IsBelowMinimum { get; init; }
    public bool IsAboveMaximum { get; init; }
}
