using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMind.Domain.Entities;

namespace StockMind.Infrastructure.Persistence.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");

        builder.HasKey(ii => ii.Id);

        builder.Property(ii => ii.InventoryId)
            .IsRequired();

        builder.Property(ii => ii.ProductId)
            .IsRequired();

        builder.Property(ii => ii.SystemQuantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ii => ii.PhysicalQuantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ii => ii.Difference)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ii => ii.Notes)
            .HasMaxLength(500);

        builder.Property(ii => ii.CreatedAt)
            .IsRequired();

        builder.Property(ii => ii.UpdatedAt);

        // Indexes
        builder.HasIndex(ii => ii.InventoryId);
        builder.HasIndex(ii => ii.ProductId);

        // Unique constraint: One product per inventory
        builder.HasIndex(ii => new { ii.InventoryId, ii.ProductId })
            .IsUnique();

        // Relationships
        builder.HasOne(ii => ii.Product)
            .WithMany()
            .HasForeignKey(ii => ii.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
