using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMind.Domain.Entities;

namespace StockMind.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.SupplierId)
            .IsRequired();

        builder.Property(po => po.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(po => po.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(po => po.OrderDate)
            .IsRequired();

        builder.Property(po => po.ExpectedDeliveryDate);

        builder.Property(po => po.ActualDeliveryDate);

        builder.Property(po => po.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(po => po.Notes)
            .HasMaxLength(1000);

        builder.Property(po => po.CreatedAt)
            .IsRequired();

        builder.Property(po => po.UpdatedAt);

        // Indexes
        builder.HasIndex(po => po.OrderNumber)
            .IsUnique();

        builder.HasIndex(po => po.SupplierId);
        builder.HasIndex(po => po.Status);
        builder.HasIndex(po => po.OrderDate);

        // Relationships
        builder.HasOne(po => po.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(po => po.Items)
            .WithOne(i => i.PurchaseOrder)
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(po => po.PurchaseEntries)
            .WithOne(pe => pe.PurchaseOrder)
            .HasForeignKey(pe => pe.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
