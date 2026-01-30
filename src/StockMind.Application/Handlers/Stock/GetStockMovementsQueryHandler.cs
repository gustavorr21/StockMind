using StockMind.Application.Common;
using StockMind.Application.DTOs.Stock;
using StockMind.Application.Queries.Stock;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Stock;

public class GetStockMovementsQueryHandler : IQueryHandler<GetStockMovementsQuery, Result<MovementListResponse>>
{
    private readonly IStockMovementRepository _movementRepository;

    public GetStockMovementsQueryHandler(IStockMovementRepository movementRepository)
    {
        _movementRepository = movementRepository;
    }

    public async Task<Result<MovementListResponse>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
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
            var (movements, totalCount) = await _movementRepository.GetMovementsAsync(
                request.Params.ProductId,
                request.Params.WarehouseId,
                request.Params.StartDate,
                request.Params.EndDate,
                request.Params.Type,
                request.Params.Origin,
                skip,
                pageSize,
                request.Params.SortBy,
                isDescending,
                cancellationToken
            );

            // Mapear para DTOs
            var items = movements.Select(m => new StockMovementDto
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

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var response = new MovementListResponse
            {
                Items = items,
                TotalItems = totalCount,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            return Result<MovementListResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<MovementListResponse>.Failure($"Error getting stock movements: {ex.Message}");
        }
    }
}
