using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMind.Domain.Entities;

namespace StockMind.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.ProductId)
            .IsRequired();

        builder.Property(sm => sm.WarehouseId)
            .IsRequired();

        builder.Property(sm => sm.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sm => sm.Origin)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sm => sm.Quantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(sm => sm.PreviousBalance)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(sm => sm.NewBalance)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(sm => sm.UserId)
            .IsRequired();

        builder.Property(sm => sm.MovementDate)
            .IsRequired();

        builder.Property(sm => sm.Observation)
            .HasMaxLength(500);

        builder.Property(sm => sm.CreatedAt)
            .IsRequired();

        builder.Property(sm => sm.UpdatedAt);

        // Indexes para otimizar consultas
        builder.HasIndex(sm => sm.ProductId)
            .HasDatabaseName("IX_StockMovement_ProductId");

        builder.HasIndex(sm => sm.WarehouseId)
            .HasDatabaseName("IX_StockMovement_WarehouseId");

        builder.HasIndex(sm => new { sm.ProductId, sm.WarehouseId })
            .HasDatabaseName("IX_StockMovement_Product_Warehouse");

        builder.HasIndex(sm => sm.MovementDate)
            .HasDatabaseName("IX_StockMovement_MovementDate");

        builder.HasIndex(sm => sm.Type)
            .HasDatabaseName("IX_StockMovement_Type");

        builder.HasIndex(sm => sm.Origin)
            .HasDatabaseName("IX_StockMovement_Origin");

        builder.HasIndex(sm => sm.UserId)
            .HasDatabaseName("IX_StockMovement_UserId");

        // Relationships
        builder.HasOne(sm => sm.Product)
            .WithMany(p => p.StockMovements)
            .HasForeignKey(sm => sm.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.Warehouse)
            .WithMany(w => w.StockMovements)
            .HasForeignKey(sm => sm.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.TransferDestinationWarehouse)
            .WithMany()
            .HasForeignKey(sm => sm.TransferDestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.PurchaseOrder)
            .WithMany()
            .HasForeignKey(sm => sm.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.PurchaseEntry)
            .WithMany(pe => pe.StockMovements)
            .HasForeignKey(sm => sm.PurchaseEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.Inventory)
            .WithMany(i => i.StockMovements)
            .HasForeignKey(sm => sm.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
