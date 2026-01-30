using StockMind.Application.Common;
using StockMind.Application.DTOs;
using StockMind.Application.Queries.Stock;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Stock;

public class GetLowStockProductsQueryHandler : IQueryHandler<GetLowStockProductsQuery, Result<IEnumerable<StockDto>>>
{
    private readonly IStockItemRepository _stockItemRepository;

    public GetLowStockProductsQueryHandler(IStockItemRepository stockItemRepository)
    {
        _stockItemRepository = stockItemRepository;
    }

    public async Task<Result<IEnumerable<StockDto>>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var stockItems = await _stockItemRepository.GetLowStockItemsAsync(cancellationToken);

            var dtos = stockItems.Select(stock => new StockDto
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
                IsLowStock = true,
                CreatedAt = stock.CreatedAt,
                UpdatedAt = stock.UpdatedAt
            });

            return Result<IEnumerable<StockDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<StockDto>>.Failure($"Error retrieving low stock items: {ex.Message}");
        }
    }
}
