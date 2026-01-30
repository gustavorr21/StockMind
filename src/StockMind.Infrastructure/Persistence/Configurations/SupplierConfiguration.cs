using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMind.Domain.Entities;
using StockMind.Domain.ValueObjects;

namespace StockMind.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.TradeName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Document)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        // Email Value Object
        builder.OwnsOne(s => s.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(100);
        });

        // Phone Value Object
        builder.OwnsOne(s => s.Phone, phone =>
        {
            phone.Property(p => p.Number)
                .HasColumnName("Phone")
                .IsRequired()
                .HasMaxLength(20);
        });

        // Address Value Object
        builder.OwnsOne(s => s.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("AddressStreet")
                .IsRequired()
                .HasMaxLength(200);

            address.Property(a => a.Number)
                .HasColumnName("AddressNumber")
                .IsRequired()
                .HasMaxLength(20);

            address.Property(a => a.Complement)
                .HasColumnName("AddressComplement")
                .HasMaxLength(100);

            address.Property(a => a.City)
                .HasColumnName("AddressCity")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasColumnName("AddressState")
                .IsRequired()
                .HasMaxLength(50);

            address.Property(a => a.ZipCode)
                .HasColumnName("AddressZipCode")
                .IsRequired()
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .HasColumnName("AddressCountry")
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.HasIndex(s => s.Document).IsUnique();
        builder.HasIndex(s => s.Status);
    }
}
