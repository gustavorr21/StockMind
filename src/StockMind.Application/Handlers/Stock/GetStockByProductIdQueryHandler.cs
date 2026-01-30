using StockMind.Application.Common;
using StockMind.Application.DTOs;
using StockMind.Application.Queries.Stock;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Stock;

public class GetStockByProductIdQueryHandler : IQueryHandler<GetStockByProductIdQuery, Result<StockDto>>
{
    private readonly IStockItemRepository _stockItemRepository;

    public GetStockByProductIdQueryHandler(IStockItemRepository stockItemRepository)
    {
        _stockItemRepository = stockItemRepository;
    }

    public async Task<Result<StockDto>> Handle(GetStockByProductIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var stock = await _stockItemRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

            if (stock == null)
            {
                return Result<StockDto>.Failure("Stock item not found for this product");
            }

            var dto = new StockDto
            {
                Id = stock.Id,
                ProductId = stock.ProductId,
                ProductName = stock.Product?.Name ?? string.Empty,
                ProductSku = stock.Product?.Sku ?? string.Empty,
                Quantity = stock.Quantity,
                ReservedQuantity = stock.ReservedQuantity,
                AvailableQuantity = stock.AvailableQuantity,
                MinimumStock = stock.Product?.MinimumStock ?? 0,
                Location = stock.Location,
                IsLowStock = stock.Quantity <= (stock.Product?.MinimumStock ?? 0),
                CreatedAt = stock.CreatedAt,
                UpdatedAt = stock.UpdatedAt
            };

            return Result<StockDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<StockDto>.Failure($"Error retrieving stock: {ex.Message}");
        }
    }
}
