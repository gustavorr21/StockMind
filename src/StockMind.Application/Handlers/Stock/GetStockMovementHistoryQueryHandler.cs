using StockMind.Application.Common;
using StockMind.Application.DTOs.Stock;
using StockMind.Application.Queries.Stock;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Stock;

public class GetStockMovementHistoryQueryHandler : IQueryHandler<GetStockMovementHistoryQuery, Result<List<StockMovementDto>>>
{
    private readonly IStockMovementRepository _movementRepository;

    public GetStockMovementHistoryQueryHandler(IStockMovementRepository movementRepository)
    {
        _movementRepository = movementRepository;
    }

    public async Task<Result<List<StockMovementDto>>> Handle(GetStockMovementHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Domain.Entities.StockMovement> movements;

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                movements = await _movementRepository.GetByDateRangeAsync(
                    request.StartDate.Value, request.EndDate.Value, cancellationToken);
                
                // Filter by product and warehouse if specified
                if (request.ProductId.HasValue)
                {
                    movements = movements.Where(m => m.ProductId == request.ProductId.Value);
                }
                if (request.WarehouseId.HasValue)
                {
                    movements = movements.Where(m => m.WarehouseId == request.WarehouseId.Value);
                }
            }
            else if (request.ProductId.HasValue && request.WarehouseId.HasValue)
            {
                movements = await _movementRepository.GetByProductAndWarehouseAsync(
                    request.ProductId.Value, request.WarehouseId.Value, cancellationToken);
            }
            else if (request.ProductId.HasValue)
            {
                movements = await _movementRepository.GetByProductIdAsync(request.ProductId.Value, cancellationToken);
            }
            else if (request.WarehouseId.HasValue)
            {
                movements = await _movementRepository.GetByWarehouseIdAsync(request.WarehouseId.Value, cancellationToken);
            }
            else
            {
                movements = await _movementRepository.GetAllAsync(cancellationToken);
            }

            var result = movements.Select(m => new StockMovementDto
            {
                Id = m.Id,
                ProductId = m.ProductId,
                ProductName = m.Product?.Name ?? string.Empty,
                ProductSku = m.Product?.Sku ?? string.Empty,
                WarehouseId = m.WarehouseId,
                WarehouseName = m.Warehouse?.Name ?? string.Empty,
                Type = m.Type.ToString(),
                Origin = m.Origin.ToString(),
                Quantity = m.Quantity,
                PreviousBalance = m.PreviousBalance,
                NewBalance = m.NewBalance,
                UserId = m.UserId,
                MovementDate = m.MovementDate,
                Observation = m.Observation
            }).ToList();

            return Result<List<StockMovementDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<StockMovementDto>>.Failure($"Error getting movement history: {ex.Message}");
        }
    }
}
