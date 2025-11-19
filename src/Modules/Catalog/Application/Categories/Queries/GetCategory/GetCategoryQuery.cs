using Common.Application;
using Modules.Catalog.Application.Categories.DTOs;
using Modules.Catalog.Domain.Categories;

namespace Modules.Catalog.Application.Categories.Queries.GetCategory;

/// <summary>
/// Query to get a category by ID
/// </summary>
public sealed record GetCategoryQuery(Guid CategoryId) : IQuery<CategoryDto?>;

/// <summary>
/// Handler for GetCategoryQuery
/// </summary>
internal sealed class GetCategoryQueryHandler : IQueryHandler<GetCategoryQuery, CategoryDto?>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto?> Handle(
        GetCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var categoryId = CategoryId.From(request.CategoryId);
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);

        if (category is null)
        {
            return null;
        }

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ParentId = category.ParentId,
            CreatedAt = category.CreatedAt
        };
    }
}
