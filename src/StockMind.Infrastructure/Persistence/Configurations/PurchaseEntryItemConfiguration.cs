using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMind.Domain.Entities;

namespace StockMind.Infrastructure.Persistence.Configurations;

public class PurchaseEntryItemConfiguration : IEntityTypeConfiguration<PurchaseEntryItem>
{
    public void Configure(EntityTypeBuilder<PurchaseEntryItem> builder)
    {
        builder.ToTable("PurchaseEntryItems");

        builder.HasKey(pei => pei.Id);

        builder.Property(pei => pei.PurchaseEntryId)
            .IsRequired();

        builder.Property(pei => pei.ProductId)
            .IsRequired();

        builder.Property(pei => pei.ReceivedQuantity)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(pei => pei.BatchNumber)
            .HasMaxLength(50);

        builder.Property(pei => pei.ExpirationDate);

        builder.Property(pei => pei.CreatedAt)
            .IsRequired();

        builder.Property(pei => pei.UpdatedAt);

        // Indexes
        builder.HasIndex(pei => pei.PurchaseEntryId);
        builder.HasIndex(pei => pei.ProductId);
        builder.HasIndex(pei => pei.BatchNumber);

        // Relationships
        builder.HasOne(pei => pei.Product)
            .WithMany()
            .HasForeignKey(pei => pei.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
