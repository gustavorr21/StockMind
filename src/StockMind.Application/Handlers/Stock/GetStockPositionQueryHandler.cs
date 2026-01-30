using StockMind.Application.Common;
using StockMind.Application.DTOs.Stock;
using StockMind.Application.Queries.Stock;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Stock;

public class GetStockPositionQueryHandler : IQueryHandler<GetStockPositionQuery, Result<List<StockPositionDto>>>
{
    private readonly IStockRepository _stockRepository;

    public GetStockPositionQueryHandler(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<Result<List<StockPositionDto>>> Handle(GetStockPositionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Domain.Entities.Stock> stocks;

            if (request.ProductId.HasValue && request.WarehouseId.HasValue)
            {
                var stock = await _stockRepository.GetByProductAndWarehouseAsync(
                    request.ProductId.Value, request.WarehouseId.Value, cancellationToken);
                
                stocks = stock != null ? new[] { stock } : Array.Empty<Domain.Entities.Stock>();
            }
            else if (request.ProductId.HasValue)
            {
                stocks = await _stockRepository.GetByProductIdAsync(request.ProductId.Value, cancellationToken);
            }
            else if (request.WarehouseId.HasValue)
            {
                stocks = await _stockRepository.GetByWarehouseIdAsync(request.WarehouseId.Value, cancellationToken);
            }
            else
            {
                stocks = await _stockRepository.GetAllAsync(cancellationToken);
            }

            var result = stocks.Select(s => new StockPositionDto
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
                IsBelowMinimum = s.Product != null && s.IsBelowMinimum(s.Product.MinimumStock),
                IsAboveMaximum = s.Product != null && s.IsAboveMaximum(s.Product.MaximumStock)
            }).ToList();

            return Result<List<StockPositionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<StockPositionDto>>.Failure($"Error getting stock position: {ex.Message}");
        }
    }
}
