namespace StockMind.Application.DTOs.Stock;

public sealed record MovementSearchParams
{
    public Guid? ProductId { get; init; }
    public Guid? WarehouseId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Type { get; init; }
    public string? Origin { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
}
