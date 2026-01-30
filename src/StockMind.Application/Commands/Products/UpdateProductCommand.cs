using StockMind.Application.Common;

namespace StockMind.Application.Commands.Products;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal PriceAmount,
    string PriceCurrency,
    decimal CostPriceAmount,
    string CostPriceCurrency,
    Guid CategoryId,
    Guid? SupplierId,
    int MinimumStock,
    string? Barcode,
    string? ImageUrl
) : ICommand<Result<bool>>;
