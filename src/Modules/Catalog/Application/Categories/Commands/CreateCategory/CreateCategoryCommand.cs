using Common.Application;
using Common.Domain;
using Modules.Catalog.Domain.Categories;

namespace Modules.Catalog.Application.Categories.Commands.CreateCategory;

/// <summary>
/// Command to create a new category
/// </summary>
public sealed record CreateCategoryCommand(
    string Name,
    string Slug,
    string? Description,
    Guid? ParentId) : ICommand<Guid>;

/// <summary>
/// Handler for CreateCategoryCommand
/// </summary>
internal sealed class CreateCategoryCommandHandler
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        // Check if slug already exists
        var slugExists = await _categoryRepository.SlugExistsAsync(
            request.Slug,
            cancellationToken);

        if (slugExists)
        {
            return Result.Failure<Guid>(new Error(
                "Category.DuplicateSlug",
                $"Category with slug '{request.Slug}' already exists"));
        }

        // Verify parent category exists if specified
        CategoryId? parentId = null;
        if (request.ParentId.HasValue)
        {
            parentId = CategoryId.From(request.ParentId.Value);
            var parentExists = await _categoryRepository.GetByIdAsync(parentId, cancellationToken);
            if (parentExists is null)
            {
                return Result.Failure<Guid>(new Error(
                    "Category.ParentNotFound",
                    $"Parent category with ID {request.ParentId} not found"));
            }
        }

        // Create category
        var category = Category.Create(request.Name, request.Slug, parentId);

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            category.UpdateInformation(request.Name, request.Description);
        }

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success((Guid)category.Id);
    }
}
