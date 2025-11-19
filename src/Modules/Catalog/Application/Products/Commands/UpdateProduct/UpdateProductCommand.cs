using Common.Application;
using Common.Domain;
using Modules.Catalog.Domain.Products;

namespace Modules.Catalog.Application.Products.Commands.UpdateProduct;

/// <summary>
/// Command to update product information
/// </summary>
public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string? Description) : ICommand;

/// <summary>
/// Handler for UpdateProductCommand
/// </summary>
internal sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var productId = ProductId.From(request.ProductId);
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(new Error(
                "Product.NotFound",
                $"Product with ID {request.ProductId} not found"));
        }

        var productName = ProductName.Create(request.Name);
        var result = product.UpdateInformation(productName, request.Description);

        if (result.IsFailure)
        {
            return result;
        }

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
