using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMind.Domain.Entities;

namespace StockMind.Infrastructure.Persistence.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("Stocks");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ProductId)
            .IsRequired();

        builder.Property(s => s.WarehouseId)
            .IsRequired();

        builder.Property(s => s.CurrentQuantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.ReservedQuantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.AvailableQuantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.LastMovementDate)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        // Indexes
        builder.HasIndex(s => new { s.ProductId, s.WarehouseId })
            .IsUnique()
            .HasDatabaseName("IX_Stock_Product_Warehouse");

        builder.HasIndex(s => s.ProductId);
        builder.HasIndex(s => s.WarehouseId);
        builder.HasIndex(s => s.CurrentQuantity);

        // Relationships
        builder.HasOne(s => s.Product)
            .WithMany(p => p.Stocks)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Warehouse)
            .WithMany(w => w.Stocks)
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
