using Common.Application;
using Common.Domain;
using Modules.Catalog.Domain.Products;

namespace Modules.Catalog.Application.Products.Commands.DiscontinueProduct;

/// <summary>
/// Command to discontinue a product
/// </summary>
public sealed record DiscontinueProductCommand(Guid ProductId) : ICommand;

/// <summary>
/// Handler for DiscontinueProductCommand
/// </summary>
internal sealed class DiscontinueProductCommandHandler
    : ICommandHandler<DiscontinueProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DiscontinueProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DiscontinueProductCommand request,
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

        var result = product.Discontinue();

        if (result.IsFailure)
        {
            return result;
        }

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
