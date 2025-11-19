namespace Modules.Catalog.Presentation.Contracts.Responses;

/// <summary>
/// Category response
/// </summary>
public sealed record CategoryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ParentId { get; init; }
    public DateTime CreatedAt { get; init; }
}
