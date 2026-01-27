using StockMind.Application.Common;
using StockMind.Application.DTOs;
using StockMind.Application.Queries.Products;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Products;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

            if (product == null)
            {
                return Result<ProductDto>.Failure("Product not found");
            }

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Sku = product.Sku,
                Barcode = product.Barcode,
                PriceAmount = product.Price.Amount,
                PriceCurrency = product.Price.Currency,
                CostPriceAmount = product.CostPrice.Amount,
                CostPriceCurrency = product.CostPrice.Currency,
                Status = product.Status,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.CompanyName,
                MinimumStock = product.MinimumStock,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            return Result<ProductDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ProductDto>.Failure($"Error retrieving product: {ex.Message}");
        }
    }
}
