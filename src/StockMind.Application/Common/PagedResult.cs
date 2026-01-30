namespace StockMind.Application.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = new List<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
