using StockMind.Domain.Enums;

namespace StockMind.Application.DTOs.Stock;

public sealed record StockMovementDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSku { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Origin { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal PreviousBalance { get; init; }
    public decimal NewBalance { get; init; }
    public Guid UserId { get; init; }
    public DateTime MovementDate { get; init; }
    public string? Observation { get; init; }
}
