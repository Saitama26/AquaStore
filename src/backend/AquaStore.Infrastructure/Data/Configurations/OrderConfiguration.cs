using AquaStore.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaStore.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(o => o.OrderNumber)
            .HasColumnName("order_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.PaymentStatus)
            .HasColumnName("payment_status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.CustomerNote)
            .HasColumnName("customer_note")
            .HasMaxLength(1000);

        builder.Property(o => o.ShippedAt)
            .HasColumnName("shipped_at");

        builder.Property(o => o.DeliveredAt)
            .HasColumnName("delivered_at");

        builder.Property(o => o.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(o => o.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(o => o.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        // Value Objects - Money
        builder.OwnsOne(o => o.SubTotal, subTotal =>
        {
            subTotal.Property(m => m.Amount)
                .HasColumnName("sub_total_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            subTotal.Property(m => m.Currency)
                .HasColumnName("sub_total_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(o => o.ShippingCost, shippingCost =>
        {
            shippingCost.Property(m => m.Amount)
                .HasColumnName("shipping_cost_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            shippingCost.Property(m => m.Currency)
                .HasColumnName("shipping_cost_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(o => o.Discount, discount =>
        {
            discount.Property(m => m.Amount)
                .HasColumnName("discount_amount")
                .HasPrecision(18, 2);

            discount.Property(m => m.Currency)
                .HasColumnName("discount_currency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(o => o.TotalAmount, total =>
        {
            total.Property(m => m.Amount)
                .HasColumnName("total_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            total.Property(m => m.Currency)
                .HasColumnName("total_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Value Object - Address
        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.City)
                .HasColumnName("shipping_city")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.Street)
                .HasColumnName("shipping_street")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(a => a.Building)
                .HasColumnName("shipping_building")
                .HasMaxLength(20)
                .IsRequired();

            address.Property(a => a.Apartment)
                .HasColumnName("shipping_apartment")
                .HasMaxLength(20);

            address.Property(a => a.PostalCode)
                .HasColumnName("shipping_postal_code")
                .HasMaxLength(20)
                .IsRequired();
        });

        // Relationships
        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index
        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.Status);
    }
}
