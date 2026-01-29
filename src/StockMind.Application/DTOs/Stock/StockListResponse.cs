namespace StockMind.Application.DTOs.Stock;

public sealed record StockListResponse
{
    public List<StockPositionDto> Items { get; init; } = new();
    public int TotalItems { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
