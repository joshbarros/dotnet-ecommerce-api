using Common.Application;
using Common.Domain;
using Modules.Catalog.Domain.Common;
using Modules.Catalog.Domain.Products;

namespace Modules.Catalog.Application.Products.Commands.UpdateProductPrice;

/// <summary>
/// Command to update product price
/// </summary>
public sealed record UpdateProductPriceCommand(
    Guid ProductId,
    decimal Price,
    string Currency) : ICommand;

/// <summary>
/// Handler for UpdateProductPriceCommand
/// </summary>
internal sealed class UpdateProductPriceCommandHandler
    : ICommandHandler<UpdateProductPriceCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductPriceCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateProductPriceCommand request,
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

        var newPrice = Money.Create(request.Price, request.Currency);
        var result = product.UpdatePrice(newPrice);

        if (result.IsFailure)
        {
            return result;
        }

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
