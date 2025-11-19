using Common.Domain;

namespace Modules.Catalog.Domain.Categories;

/// <summary>
/// Category aggregate root
/// Represents a product category with hierarchical support
/// </summary>
public sealed class Category : AggregateRoot<CategoryId>
{
    private Category(
        CategoryId id,
        string name,
        string slug,
        CategoryId? parentId)
        : base(id)
    {
        Name = name;
        Slug = slug;
        ParentId = parentId;
        CreatedAt = DateTime.UtcNow;
    }

    private Category() : base()
    {
        // Required by EF Core
    }

    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public CategoryId? ParentId { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private init; }

    /// <summary>
    /// Factory method to create a new category
    /// </summary>
    public static Category Create(string name, string slug, CategoryId? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("Category slug cannot be empty");
        }

        if (name.Length > 100)
        {
            throw new DomainException("Category name cannot exceed 100 characters");
        }

        var categoryId = CategoryId.New();
        var category = new Category(categoryId, name.Trim(), slug.ToLowerInvariant(), parentId);

        category.RaiseDomainEvent(new CategoryCreatedEvent(categoryId, name, slug));

        return category;
    }

    /// <summary>
    /// Updates category information
    /// </summary>
    public Result UpdateInformation(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(new Error(
                "Category.InvalidName",
                "Category name cannot be empty"));
        }

        if (name.Length > 100)
        {
            return Result.Failure(new Error(
                "Category.NameTooLong",
                "Category name cannot exceed 100 characters"));
        }

        Name = name.Trim();
        Description = description;

        RaiseDomainEvent(new CategoryUpdatedEvent(Id, name));

        return Result.Success();
    }
}

// Category Domain Events

public sealed record CategoryCreatedEvent(
    Guid CategoryId,
    string Name,
    string Slug) : DomainEvent;

public sealed record CategoryUpdatedEvent(
    Guid CategoryId,
    string Name) : DomainEvent;
