using AquaStore.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaStore.Infrastructure.Data.Configurations;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("user_addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false);

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        // Value Object
        builder.OwnsOne(a => a.Address, address =>
        {
            address.Property(ad => ad.City)
                .HasColumnName("city")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(ad => ad.Street)
                .HasColumnName("street")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(ad => ad.Building)
                .HasColumnName("building")
                .HasMaxLength(20)
                .IsRequired();

            address.Property(ad => ad.Apartment)
                .HasColumnName("apartment")
                .HasMaxLength(20);

            address.Property(ad => ad.PostalCode)
                .HasColumnName("postal_code")
                .HasMaxLength(20)
                .IsRequired();
        });

        // Index
        builder.HasIndex(a => a.UserId);
    }
}
