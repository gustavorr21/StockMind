using StockMind.Application.Common;

namespace StockMind.Application.Commands.Products;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    string Sku,
    decimal PriceAmount,
    string PriceCurrency,
    decimal CostPriceAmount,
    string CostPriceCurrency,
    Guid CategoryId,
    Guid? SupplierId,
    int MinimumStock,
    string? Barcode,
    string? ImageUrl
) : ICommand<Result<Guid>>;
