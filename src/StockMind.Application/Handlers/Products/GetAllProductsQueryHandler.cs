using StockMind.Application.Common;
using StockMind.Application.DTOs;
using StockMind.Application.Queries.Products;
using StockMind.Domain.Repositories;

namespace StockMind.Application.Handlers.Products;

public class GetAllProductsQueryHandler : IQueryHandler<GetAllProductsQuery, Result<IEnumerable<ProductDto>>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<IEnumerable<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var products = await _productRepository.GetAllAsync(cancellationToken);

            var dtos = products.Select(product => new ProductDto
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
                Status = product.Status.ToString(),
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.CompanyName,
                MinimumStock = product.MinimumStock,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            });

            return Result<IEnumerable<ProductDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ProductDto>>.Failure($"Error retrieving products: {ex.Message}");
        }
    }
}
