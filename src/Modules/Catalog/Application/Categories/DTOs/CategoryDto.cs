namespace Modules.Catalog.Application.Categories.DTOs;

/// <summary>
/// Category data transfer object
/// </summary>
public sealed record CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ParentId { get; init; }
    public string? ParentName { get; init; }
    public DateTime CreatedAt { get; init; }
}
