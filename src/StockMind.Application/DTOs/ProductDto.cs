using StockMind.Domain.Enums;

namespace StockMind.Application.DTOs;

public sealed record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public decimal PriceAmount { get; init; }
    public string PriceCurrency { get; init; } = string.Empty;
    public decimal CostPriceAmount { get; init; }
    public string CostPriceCurrency { get; init; } = string.Empty;
    public ProductStatus Status { get; init; }
    public Guid CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public Guid? SupplierId { get; init; }
    public string? SupplierName { get; init; }
    public int MinimumStock { get; init; }
    public string? ImageUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

