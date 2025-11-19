using Common.Application;
using Common.Domain;
using Modules.Catalog.Domain.Categories;
using Modules.Catalog.Domain.Common;
using Modules.Catalog.Domain.Products;

namespace Modules.Catalog.Application.Products.Commands.CreateProduct;

/// <summary>
/// Command to create a new product
/// </summary>
public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    Guid CategoryId) : ICommand<Guid>;

/// <summary>
/// Handler for CreateProductCommand
/// </summary>
internal sealed class CreateProductCommandHandler
    : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        // Verify category exists
        var categoryId = CategoryId.From(request.CategoryId);
        var categoryExists = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (categoryExists is null)
        {
            return Result.Failure<Guid>(new Error(
                "Category.NotFound",
                $"Category with ID {request.CategoryId} not found"));
        }

        // Create domain objects
        var productName = ProductName.Create(request.Name);
        var price = Money.Create(request.Price, request.Currency);

        // Create product aggregate
        var product = Product.Create(
            productName,
            request.Description,
            price,
            categoryId);

        // Persist
        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success((Guid)product.Id);
    }
}
