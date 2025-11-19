using Common.Application;
using Common.Domain;
using Modules.Catalog.Domain.Products;

namespace Modules.Catalog.Application.Products.Commands.ActivateProduct;

/// <summary>
/// Command to activate a product
/// </summary>
public sealed record ActivateProductCommand(Guid ProductId) : ICommand;

/// <summary>
/// Handler for ActivateProductCommand
/// </summary>
internal sealed class ActivateProductCommandHandler : ICommandHandler<ActivateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ActivateProductCommand request,
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

        var result = product.Activate();

        if (result.IsFailure)
        {
            return result;
        }

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
