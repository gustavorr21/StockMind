using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMind.Domain.Entities;

namespace StockMind.Infrastructure.Persistence.Configurations;

public class PurchaseEntryConfiguration : IEntityTypeConfiguration<PurchaseEntry>
{
    public void Configure(EntityTypeBuilder<PurchaseEntry> builder)
    {
        builder.ToTable("PurchaseEntries");

        builder.HasKey(pe => pe.Id);

        builder.Property(pe => pe.PurchaseOrderId)
            .IsRequired();

        builder.Property(pe => pe.WarehouseId)
            .IsRequired();

        builder.Property(pe => pe.EntryDate)
            .IsRequired();

        builder.Property(pe => pe.InvoiceNumber)
            .HasMaxLength(100);

        builder.Property(pe => pe.Notes)
            .HasMaxLength(1000);

        builder.Property(pe => pe.ReceivedByUserId)
            .IsRequired();

        builder.Property(pe => pe.IsConfirmed)
            .IsRequired();

        builder.Property(pe => pe.ConfirmedAt);

        builder.Property(pe => pe.CreatedAt)
            .IsRequired();

        builder.Property(pe => pe.UpdatedAt);

        // Indexes
        builder.HasIndex(pe => pe.PurchaseOrderId);
        builder.HasIndex(pe => pe.WarehouseId);
        builder.HasIndex(pe => pe.EntryDate);
        builder.HasIndex(pe => pe.InvoiceNumber);

        // Relationships
        builder.HasOne(pe => pe.Warehouse)
            .WithMany()
            .HasForeignKey(pe => pe.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(pe => pe.Items)
            .WithOne(i => i.PurchaseEntry)
            .HasForeignKey(i => i.PurchaseEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
