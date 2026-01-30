using StockMind.Application.Common;
using StockMind.Application.DTOs.Stock;
using StockMind.Application.Queries.Stock;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Stock;

public class GetStockItemsQueryHandler : IQueryHandler<GetStockItemsQuery, Result<StockListResponse>>
{
    private readonly IStockRepository _stockRepository;

    public GetStockItemsQueryHandler(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<Result<StockListResponse>> Handle(GetStockItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Paginação
            var page = request.Params.Page < 1 ? 1 : request.Params.Page;
            var pageSize = request.Params.PageSize < 1 ? 20 :
                          request.Params.PageSize > 100 ? 100 : request.Params.PageSize;

            var skip = (page - 1) * pageSize;
            var isDescending = request.Params.SortOrder?.ToLower() == "desc";

            // Buscar direto do repositório com filtros aplicados no SQL
            var (stocks, totalCount) = await _stockRepository.GetStockItemsAsync(
                request.Params.SearchTerm,
                request.Params.ProductId,
                request.Params.CategoryId,
                request.Params.WarehouseId,
                request.Params.Status,
                request.Params.OnlyLowStock,
                request.Params.OnlyCritical,
                skip,
                pageSize,
                request.Params.SortBy,
                isDescending,
                cancellationToken
            );

            // Mapear para DTOs
            var items = stocks.Select(s => new StockPositionDto
            {
                StockId = s.Id,
                ProductId = s.ProductId,
                ProductName = s.Product?.Name ?? string.Empty,
                ProductSku = s.Product?.Sku ?? string.Empty,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse?.Name ?? string.Empty,
                CurrentQuantity = s.CurrentQuantity,
                ReservedQuantity = s.ReservedQuantity,
                AvailableQuantity = s.AvailableQuantity,
                MinimumStock = s.Product?.MinimumStock ?? 0,
                MaximumStock = s.Product?.MaximumStock ?? 0,
                LastMovementDate = s.LastMovementDate,
                IsBelowMinimum = s.Product != null && s.CurrentQuantity < s.Product.MinimumStock,
                IsAboveMaximum = s.Product != null && s.Product.MaximumStock > 0 && s.CurrentQuantity > s.Product.MaximumStock
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var response = new StockListResponse
            {
                Items = items,
                TotalItems = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Result<StockListResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<StockListResponse>.Failure($"Error getting stock items: {ex.Message}");
        }
    }
}
