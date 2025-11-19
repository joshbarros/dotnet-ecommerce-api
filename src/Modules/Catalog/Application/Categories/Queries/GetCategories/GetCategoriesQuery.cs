using Common.Application;
using Modules.Catalog.Application.Categories.DTOs;

namespace Modules.Catalog.Application.Categories.Queries.GetCategories;

/// <summary>
/// Query to get all categories (optionally filtered by parent)
/// </summary>
public sealed record GetCategoriesQuery(Guid? ParentId = null) : IQuery<List<CategoryDto>>;

/// <summary>
/// Handler for GetCategoriesQuery
/// </summary>
internal sealed class GetCategoriesQueryHandler
    : IQueryHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Categories.Category> categories;

        if (request.ParentId.HasValue)
        {
            var parentId = Domain.Categories.CategoryId.From(request.ParentId.Value);
            categories = await _categoryRepository.GetChildrenAsync(parentId, cancellationToken);
        }
        else
        {
            // TODO: Implement GetAllAsync in repository
            return new List<CategoryDto>();
        }

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            ParentId = c.ParentId,
            CreatedAt = c.CreatedAt
        }).ToList();
    }
}
