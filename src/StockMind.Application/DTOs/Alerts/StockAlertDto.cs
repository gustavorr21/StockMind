namespace StockMind.Application.DTOs.Alerts;

public sealed record StockAlertDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSku { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public decimal CurrentQuantity { get; init; }
    public decimal MinimumQuantity { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime FirstDetectedAt { get; init; }
    public DateTime LastNotifiedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public DateTime? AcknowledgedAt { get; init; }
    public string? Notes { get; init; }
}
