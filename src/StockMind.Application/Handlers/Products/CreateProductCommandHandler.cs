using StockMind.Application.Commands.Products;
using StockMind.Application.Common;
using StockMind.Domain.Entities;
using StockMind.Domain.Repositories;
using StockMind.Domain.ValueObjects;

namespace StockMind.Application.Handlers.Products;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if SKU already exists
            if (await _productRepository.ExistsSkuAsync(request.Sku, cancellationToken))
            {
                return Result<Guid>.Failure($"Product with SKU '{request.Sku}' already exists");
            }

            // Verify category exists
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<Guid>.Failure("Category not found");
            }

            // Create Money value objects
            var price = Money.Create(request.PriceAmount, request.PriceCurrency);
            var costPrice = Money.Create(request.CostPriceAmount, request.CostPriceCurrency);

            // Create product
            var product = Product.Create(
                request.Name,
                request.Description,
                request.Sku,
                price,
                costPrice,
                request.CategoryId,
                request.MinimumStock
            );

            if (!string.IsNullOrWhiteSpace(request.Barcode))
            {
                product.SetBarcode(request.Barcode);
            }

            if (request.SupplierId.HasValue)
            {
                product.AssignSupplier(request.SupplierId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                product.SetImageUrl(request.ImageUrl);
            }

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(product.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Error creating product: {ex.Message}");
        }
    }
}
