namespace StockMind.Application.DTOs.Stock;

public sealed record StockSearchParams
{
    public string? SearchTerm { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? WarehouseId { get; init; }
    public string? Status { get; init; }
    public bool? OnlyLowStock { get; init; }
    public bool? OnlyCritical { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
}
