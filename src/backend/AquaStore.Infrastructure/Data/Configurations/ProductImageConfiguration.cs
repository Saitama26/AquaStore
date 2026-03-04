using AquaStore.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaStore.Infrastructure.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(i => i.Url)
            .HasColumnName("url")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(i => i.AltText)
            .HasColumnName("alt_text")
            .HasMaxLength(200);

        builder.Property(i => i.IsMain)
            .HasColumnName("is_main")
            .HasDefaultValue(false);

        builder.Property(i => i.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0);

        builder.Property(i => i.ProductId)
            .HasColumnName("product_id")
            .IsRequired();
    }
}

