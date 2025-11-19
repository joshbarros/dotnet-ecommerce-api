using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Catalog.Domain.Categories;
using Modules.Catalog.Domain.Products;

namespace Modules.Catalog.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Product aggregate
/// </summary>
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => ProductId.From(value))
            .HasColumnName("id");

        // Configure ProductName value object
        builder.Property(p => p.Name)
            .HasConversion(
                name => name.Value,
                value => ProductName.Create(value))
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        // Configure Money value object (owned type)
        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("price_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("price_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(p => p.CategoryId)
            .HasConversion(
                id => id.Value,
                value => CategoryId.From(value))
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Configure ProductImage collection
        builder.OwnsMany(p => p.Images, imageBuilder =>
        {
            imageBuilder.ToTable("product_images");

            imageBuilder.WithOwner()
                .HasForeignKey("ProductId");

            imageBuilder.HasKey(nameof(ProductImage.Id));

            imageBuilder.Property(pi => pi.Id)
                .HasColumnName("id");

            imageBuilder.Property<Guid>("ProductId")
                .HasColumnName("product_id")
                .IsRequired();

            imageBuilder.Property(pi => pi.Url)
                .HasColumnName("url")
                .HasMaxLength(500)
                .IsRequired();

            imageBuilder.Property(pi => pi.DisplayOrder)
                .HasColumnName("display_order")
                .IsRequired();
        });

        // Indexes for performance
        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("ix_products_category_id");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("ix_products_status");

        builder.HasIndex(p => p.Name)
            .HasDatabaseName("ix_products_name");

        // Composite index for common queries
        builder.HasIndex(p => new { p.CategoryId, p.Status })
            .HasDatabaseName("ix_products_category_status");

        // Ignore domain events (not persisted)
        builder.Ignore(p => p.DomainEvents);
    }
}
