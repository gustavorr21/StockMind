using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMind.Domain.Entities;

namespace StockMind.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Barcode)
            .HasMaxLength(50);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.Property(p => p.SupplierId);

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        // Novos campos do sistema de estoque profissional
        builder.Property(p => p.UnitOfMeasure)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.MinimumStock)
            .IsRequired();

        builder.Property(p => p.MaximumStock)
            .IsRequired();

        builder.Property(p => p.ControlsBatch)
            .IsRequired();

        builder.Property(p => p.ControlsExpiration)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        // Price Value Object
        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("PriceAmount")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            price.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .IsRequired()
                .HasMaxLength(3);
        });

        // CostPrice Value Object
        builder.OwnsOne(p => p.CostPrice, costPrice =>
        {
            costPrice.Property(m => m.Amount)
                .HasColumnName("CostPriceAmount")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            costPrice.Property(m => m.Currency)
                .HasColumnName("CostPriceCurrency")
                .IsRequired()
                .HasMaxLength(3);
        });

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Stocks)
            .WithOne(s => s.Product)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.StockMovements)
            .WithOne(sm => sm.Product)
            .HasForeignKey(sm => sm.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.Barcode);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Status);

        // Ignore Domain Events (not persisted)
        builder.Ignore(p => p.DomainEvents);
    }
}
