using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Customers.Domain.Common;
using Modules.Customers.Domain.Customers;

namespace Modules.Customers.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => CustomerId.Create(value))
            .HasColumnName("id");

        // CustomerName complex type
        builder.ComplexProperty(c => c.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("first_name");

            nameBuilder.Property(n => n.LastName)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("last_name");
        });

        // Email value object
        builder.Property(c => c.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasMaxLength(254)
            .IsRequired()
            .HasColumnName("email");

        // PhoneNumber value object
        builder.Property(c => c.PhoneNumber)
            .HasConversion(
                phone => phone != null ? phone.Value : null,
                value => value != null ? PhoneNumber.Create(value) : null)
            .HasMaxLength(20)
            .HasColumnName("phone_number");

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasColumnName("status");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // Addresses - owned entity collection
        builder.OwnsMany(c => c.Addresses, addressBuilder =>
        {
            addressBuilder.ToTable("customer_addresses");

            addressBuilder.WithOwner().HasForeignKey("customer_id");

            addressBuilder.Property<int>("id")
                .ValueGeneratedOnAdd();

            addressBuilder.HasKey("id");

            addressBuilder.Property(a => a.Street)
                .HasMaxLength(200)
                .IsRequired()
                .HasColumnName("street");

            addressBuilder.Property(a => a.City)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("city");

            addressBuilder.Property(a => a.State)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("state");

            addressBuilder.Property(a => a.Country)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("country");

            addressBuilder.Property(a => a.PostalCode)
                .HasMaxLength(20)
                .IsRequired()
                .HasColumnName("postal_code");
        });

        // Indexes
        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.CreatedAt);

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);
    }
}
