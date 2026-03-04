using AquaStore.Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaStore.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.Comment)
            .HasColumnName("comment")
            .HasMaxLength(2000);

        builder.Property(r => r.IsApproved)
            .HasColumnName("is_approved")
            .HasDefaultValue(false);

        builder.Property(r => r.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(r => r.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        // Value Object
        builder.OwnsOne(r => r.Rating, rating =>
        {
            rating.Property(rt => rt.Value)
                .HasColumnName("rating")
                .IsRequired();
        });

        // Relationship
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for preventing duplicate reviews
        builder.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
    }
}
