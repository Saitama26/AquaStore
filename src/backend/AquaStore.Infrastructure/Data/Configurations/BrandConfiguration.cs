using AquaStore.Domain.Brands;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaStore.Infrastructure.Data.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("brands");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(b => b.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(b => b.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(b => b.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500);

        builder.Property(b => b.WebsiteUrl)
            .HasColumnName("website_url")
            .HasMaxLength(500);

        builder.Property(b => b.Country)
            .HasColumnName("country")
            .HasMaxLength(100);

        builder.Property(b => b.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(b => b.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        // Value Object
        builder.OwnsOne(b => b.Slug, slug =>
        {
            slug.Property(s => s.Value)
                .HasColumnName("slug")
                .HasMaxLength(150)
                .IsRequired();

            slug.HasIndex(s => s.Value).IsUnique();
        });
    }
}
