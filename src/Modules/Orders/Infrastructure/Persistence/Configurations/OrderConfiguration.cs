using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Catalog.Domain.Common;
using Modules.Catalog.Domain.Products;
using Modules.Customers.Domain.Common;
using Modules.Customers.Domain.Customers;
using Modules.Orders.Domain.Orders;

namespace Modules.Orders.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => OrderId.Create(value))
            .HasColumnName("id");

        // CustomerId value object
        builder.Property(o => o.CustomerId)
            .HasConversion(
                id => id.Value,
                value => CustomerId.Create(value))
            .IsRequired()
            .HasColumnName("customer_id");

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasColumnName("status");

        // TotalAmount complex type
        builder.ComplexProperty(o => o.TotalAmount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasPrecision(18, 2)
                .IsRequired()
                .HasColumnName("total_amount");

            moneyBuilder.Property(m => m.Currency)
                .HasMaxLength(3)
                .IsRequired()
                .HasColumnName("currency");
        });

        // ShippingAddress complex type
        builder.ComplexProperty(o => o.ShippingAddress, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street)
                .HasMaxLength(200)
                .IsRequired()
                .HasColumnName("shipping_street");

            addressBuilder.Property(a => a.City)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("shipping_city");

            addressBuilder.Property(a => a.State)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("shipping_state");

            addressBuilder.Property(a => a.Country)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("shipping_country");

            addressBuilder.Property(a => a.PostalCode)
                .HasMaxLength(20)
                .IsRequired()
                .HasColumnName("shipping_postal_code");
        });

        builder.Property(o => o.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(o => o.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.Property(o => o.ShippedAt)
            .HasColumnName("shipped_at");

        builder.Property(o => o.DeliveredAt)
            .HasColumnName("delivered_at");

        builder.Property(o => o.CancelledAt)
            .HasColumnName("cancelled_at");

        // OrderItems - owned entity collection
        builder.OwnsMany(o => o.Items, itemBuilder =>
        {
            itemBuilder.ToTable("order_items");

            itemBuilder.WithOwner().HasForeignKey("order_id");

            itemBuilder.HasKey("Id");

            itemBuilder.Property(i => i.Id)
                .HasConversion(
                    id => id.Value,
                    value => new OrderItemId(value))
                .HasColumnName("id");

            itemBuilder.Property(i => i.ProductId)
                .HasConversion(
                    id => id.Value,
                    value => ProductId.Create(value))
                .IsRequired()
                .HasColumnName("product_id");

            itemBuilder.Property(i => i.ProductName)
                .HasMaxLength(200)
                .IsRequired()
                .HasColumnName("product_name");

            itemBuilder.Property(i => i.Quantity)
                .IsRequired()
                .HasColumnName("quantity");

            // UnitPrice complex type
            itemBuilder.ComplexProperty(i => i.UnitPrice, priceBuilder =>
            {
                priceBuilder.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired()
                    .HasColumnName("unit_price");

                priceBuilder.Property(m => m.Currency)
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasColumnName("currency");
            });

            // Subtotal complex type
            itemBuilder.ComplexProperty(i => i.Subtotal, subtotalBuilder =>
            {
                subtotalBuilder.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired()
                    .HasColumnName("subtotal");

                subtotalBuilder.Property(m => m.Currency)
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasColumnName("subtotal_currency");
            });

            itemBuilder.HasIndex(i => i.ProductId);
        });

        // Indexes
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
        builder.HasIndex(o => new { o.CustomerId, o.Status });

        // Ignore domain events
        builder.Ignore(o => o.DomainEvents);
    }
}
