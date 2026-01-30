using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMind.Domain.Entities;

namespace StockMind.Infrastructure.Persistence.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventories");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.WarehouseId)
            .IsRequired();

        builder.Property(i => i.InventoryNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.StartDate)
            .IsRequired();

        builder.Property(i => i.EndDate);

        builder.Property(i => i.StartedByUserId)
            .IsRequired();

        builder.Property(i => i.ApprovedByUserId);

        builder.Property(i => i.ApprovedAt);

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.UpdatedAt);

        // Indexes
        builder.HasIndex(i => i.InventoryNumber)
            .IsUnique();

        builder.HasIndex(i => i.WarehouseId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.StartDate);

        // Relationships
        builder.HasMany(i => i.Items)
            .WithOne(item => item.Inventory)
            .HasForeignKey(item => item.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
