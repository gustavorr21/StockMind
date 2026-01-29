using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMind.Domain.Entities;

namespace StockMind.Infrastructure.Persistence.Configurations;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");

        builder.HasKey(poi => poi.Id);

        builder.Property(poi => poi.PurchaseOrderId)
            .IsRequired();

        builder.Property(poi => poi.ProductId)
            .IsRequired();

        builder.Property(poi => poi.Quantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(poi => poi.UnitCost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(poi => poi.TotalCost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(poi => poi.ReceivedQuantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(poi => poi.CreatedAt)
            .IsRequired();

        builder.Property(poi => poi.UpdatedAt);

        // Indexes
        builder.HasIndex(poi => poi.PurchaseOrderId);
        builder.HasIndex(poi => poi.ProductId);

        // Relationships
        builder.HasOne(poi => poi.Product)
            .WithMany()
            .HasForeignKey(poi => poi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
