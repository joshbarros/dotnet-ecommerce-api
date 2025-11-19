namespace Modules.Catalog.Presentation.Contracts.Requests;

/// <summary>
/// Request to create a new category
/// </summary>
public sealed record CreateCategoryRequest
{
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ParentId { get; init; }
}
