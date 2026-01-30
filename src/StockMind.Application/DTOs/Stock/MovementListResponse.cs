namespace StockMind.Application.DTOs.Stock;

public sealed record MovementListResponse
{
    public List<StockMovementDto> Items { get; init; } = new();
    public int TotalItems { get; init; }
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
    public int PageSize { get; init; }
}
