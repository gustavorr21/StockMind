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
            // Buscar todos os stocks com filtros básicos
            IEnumerable<Domain.Entities.Stock> stocks;

            if (request.Params.ProductId.HasValue && request.Params.WarehouseId.HasValue)
            {
                var stock = await _stockRepository.GetByProductAndWarehouseAsync(
                    request.Params.ProductId.Value, request.Params.WarehouseId.Value, cancellationToken);
                stocks = stock != null ? new[] { stock } : Array.Empty<Domain.Entities.Stock>();
            }
            else if (request.Params.ProductId.HasValue)
            {
                stocks = await _stockRepository.GetByProductIdAsync(request.Params.ProductId.Value, cancellationToken);
            }
            else if (request.Params.WarehouseId.HasValue)
            {
                stocks = await _stockRepository.GetByWarehouseIdAsync(request.Params.WarehouseId.Value, cancellationToken);
            }
            else
            {
                stocks = await _stockRepository.GetAllAsync(cancellationToken);
            }

            // Aplicar filtros em memória
            var filteredStocks = stocks.AsQueryable();

            // Filtro por termo de busca
            if (!string.IsNullOrWhiteSpace(request.Params.SearchTerm))
            {
                var searchTerm = request.Params.SearchTerm.ToLower();
                filteredStocks = filteredStocks.Where(s =>
                    s.Product.Name.ToLower().Contains(searchTerm) ||
                    s.Product.Sku.ToLower().Contains(searchTerm));
            }

            // Filtro por categoria
            if (request.Params.CategoryId.HasValue)
            {
                filteredStocks = filteredStocks.Where(s => s.Product.CategoryId == request.Params.CategoryId.Value);
            }

            // Filtro por status
            if (!string.IsNullOrWhiteSpace(request.Params.Status))
            {
                if (Enum.TryParse<Domain.Enums.ProductStatus>(request.Params.Status, out var status))
                {
                    filteredStocks = filteredStocks.Where(s => s.Product.Status == status);
                }
            }

            // Filtro por estoque baixo
            if (request.Params.OnlyLowStock == true)
            {
                filteredStocks = filteredStocks.Where(s => s.CurrentQuantity < s.Product.MinimumStock);
            }

            // Filtro por estoque crítico
            if (request.Params.OnlyCritical == true)
            {
                filteredStocks = filteredStocks.Where(s => s.CurrentQuantity == 0);
            }

            // Ordenação
            filteredStocks = ApplySorting(filteredStocks, request.Params.SortBy, request.Params.SortOrder);

            // Total de itens
            var totalItems = filteredStocks.Count();

            // Paginação
            var page = request.Params.Page < 1 ? 1 : request.Params.Page;
            var pageSize = request.Params.PageSize < 1 ? 20 :
                          request.Params.PageSize > 100 ? 100 : request.Params.PageSize;

            // Materializar os dados antes do Select
            var pagedStocks = filteredStocks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Agora aplicar o Select nos dados materializados
            var items = pagedStocks
                .Select(s => new StockPositionDto
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
                })
                .ToList();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var response = new StockListResponse
            {
                Items = items,
                TotalItems = totalItems,
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

    private IQueryable<Domain.Entities.Stock> ApplySorting(
        IQueryable<Domain.Entities.Stock> query,
        string? sortBy,
        string? sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "productname" => isDescending
                ? query.OrderByDescending(s => s.Product.Name)
                : query.OrderBy(s => s.Product.Name),

            "sku" => isDescending
                ? query.OrderByDescending(s => s.Product.Sku)
                : query.OrderBy(s => s.Product.Sku),

            "warehouse" => isDescending
                ? query.OrderByDescending(s => s.Warehouse.Name)
                : query.OrderBy(s => s.Warehouse.Name),

            "currentquantity" => isDescending
                ? query.OrderByDescending(s => s.CurrentQuantity)
                : query.OrderBy(s => s.CurrentQuantity),

            "availablequantity" => isDescending
                ? query.OrderByDescending(s => s.AvailableQuantity)
                : query.OrderBy(s => s.AvailableQuantity),

            "lastmovementdate" => isDescending
                ? query.OrderByDescending(s => s.LastMovementDate)
                : query.OrderBy(s => s.LastMovementDate),

            _ => query.OrderBy(s => s.Product.Name) // Default
        };
    }
}
