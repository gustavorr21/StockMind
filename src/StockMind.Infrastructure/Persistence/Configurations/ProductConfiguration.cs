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

        builder.Property(p => p.MinimumStock)
            .IsRequired();

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

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
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.Barcode);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Status);

        // Ignore Domain Events (not persisted)
        builder.Ignore(p => p.DomainEvents);
    }
}
