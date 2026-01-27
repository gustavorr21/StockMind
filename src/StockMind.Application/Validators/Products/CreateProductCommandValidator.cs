using FluentValidation;
using StockMind.Application.Commands.Products;

namespace StockMind.Application.Validators.Products;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
            .Matches("^[A-Z0-9-]+$").WithMessage("SKU must contain only uppercase letters, numbers, and hyphens");

        RuleFor(x => x.PriceAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative");

        RuleFor(x => x.CostPriceAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price cannot be negative");

        RuleFor(x => x)
            .Must(x => x.PriceAmount >= x.CostPriceAmount)
            .WithMessage("Sale price must be greater than or equal to cost price");

        RuleFor(x => x.PriceCurrency)
            .NotEmpty().WithMessage("Price currency is required")
            .Length(3).WithMessage("Currency must be 3 characters (e.g., BRL, USD)");

        RuleFor(x => x.CostPriceCurrency)
            .NotEmpty().WithMessage("Cost price currency is required")
            .Length(3).WithMessage("Currency must be 3 characters (e.g., BRL, USD)");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.MinimumStock)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock cannot be negative");

        RuleFor(x => x.Barcode)
            .MaximumLength(50).WithMessage("Barcode cannot exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Barcode));

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters")
            .Must(BeAValidUrl).WithMessage("Image URL must be a valid URL")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

