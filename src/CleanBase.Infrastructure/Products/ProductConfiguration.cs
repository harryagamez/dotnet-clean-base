using CleanBase.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanBase.Infrastructure.Products;

/// <summary>Maps <see cref="Product"/> to the database without polluting the entity with attributes.</summary>
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();

        builder.Property(product => product.Name)
            .HasMaxLength(Product.NameMaxLength)
            .IsRequired();

        builder.Property(product => product.Sku)
            .HasMaxLength(Product.SkuMaxLength)
            .IsRequired();

        builder.HasIndex(product => product.Sku).IsUnique();

        builder.Property(product => product.Price).HasPrecision(18, 2);
    }
}
