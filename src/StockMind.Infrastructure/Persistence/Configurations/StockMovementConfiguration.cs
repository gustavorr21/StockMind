using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMind.Domain.Entities;

namespace StockMind.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.ProductId)
            .IsRequired();

        builder.Property(s => s.MovementType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.Quantity)
            .IsRequired();

        builder.Property(s => s.PreviousQuantity)
            .IsRequired();

        builder.Property(s => s.NewQuantity)
            .IsRequired();

        builder.Property(s => s.Reference)
            .HasMaxLength(100);

        builder.Property(s => s.Notes)
            .HasMaxLength(500);

        builder.Property(s => s.UserId);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        // Relationship
        builder.HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(s => s.ProductId);
        builder.HasIndex(s => s.MovementType);
        builder.HasIndex(s => s.CreatedAt);
    }
}
