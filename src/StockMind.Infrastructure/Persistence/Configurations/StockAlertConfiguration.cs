using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMind.Domain.Entities;

namespace StockMind.Infrastructure.Persistence.Configurations;

public class StockAlertConfiguration : IEntityTypeConfiguration<StockAlert>
{
    public void Configure(EntityTypeBuilder<StockAlert> builder)
    {
        builder.ToTable("StockAlerts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ProductId)
            .IsRequired();

        builder.Property(a => a.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.ProductSku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.WarehouseId)
            .IsRequired();

        builder.Property(a => a.WarehouseName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.CurrentQuantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(a => a.MinimumQuantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.FirstDetectedAt)
            .IsRequired();

        builder.Property(a => a.LastNotifiedAt)
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(a => new { a.ProductId, a.WarehouseId, a.Status })
            .HasDatabaseName("IX_StockAlert_Product_Warehouse_Status");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_StockAlert_Status");

        builder.HasIndex(a => a.FirstDetectedAt)
            .HasDatabaseName("IX_StockAlert_FirstDetectedAt");

        // Relationships
        builder.HasOne(a => a.Product)
            .WithMany()
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Warehouse)
            .WithMany()
            .HasForeignKey(a => a.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Ignore DomainEvents
        builder.Ignore(a => a.DomainEvents);
    }
}
