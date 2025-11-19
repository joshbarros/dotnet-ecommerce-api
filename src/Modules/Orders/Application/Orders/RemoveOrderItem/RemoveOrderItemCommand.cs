using Common.Application;
using Common.Domain;
using FluentValidation;
using Modules.Catalog.Domain.Products;
using Modules.Orders.Application.Abstractions.Data;
using Modules.Orders.Domain.Orders;

namespace Modules.Orders.Application.Orders.RemoveOrderItem;

public sealed record RemoveOrderItemCommand(
    Guid OrderId,
    Guid ProductId) : ICommand;

internal sealed class RemoveOrderItemCommandHandler : ICommandHandler<RemoveOrderItemCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrdersUnitOfWork _unitOfWork;

    public RemoveOrderItemCommandHandler(
        IOrderRepository orderRepository,
        IOrdersUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(
            OrderId.Create(request.OrderId),
            cancellationToken);

        if (order is null)
        {
            return Result.Failure(new Error(
                "Order.NotFound",
                $"Order with ID '{request.OrderId}' was not found"));
        }

        var productId = ProductId.Create(request.ProductId);
        var result = order.RemoveItem(productId);

        if (result.IsFailure)
        {
            return result;
        }

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class RemoveOrderItemCommandValidator : AbstractValidator<RemoveOrderItemCommand>
{
    public RemoveOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
    }
}
