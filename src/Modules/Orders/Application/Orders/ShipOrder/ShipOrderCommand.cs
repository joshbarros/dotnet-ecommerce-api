using Common.Application;
using Common.Domain;
using FluentValidation;
using Modules.Orders.Application.Abstractions.Data;
using Modules.Orders.Domain.Orders;

namespace Modules.Orders.Application.Orders.ShipOrder;

public sealed record ShipOrderCommand(Guid OrderId) : ICommand;

internal sealed class ShipOrderCommandHandler : ICommandHandler<ShipOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrdersUnitOfWork _unitOfWork;

    public ShipOrderCommandHandler(
        IOrderRepository orderRepository,
        IOrdersUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
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

        // First start processing if not already
        if (order.Status == OrderStatus.Confirmed)
        {
            var processResult = order.StartProcessing();
            if (processResult.IsFailure)
            {
                return processResult;
            }
        }

        var result = order.Ship();

        if (result.IsFailure)
        {
            return result;
        }

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
{
    public ShipOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");
    }
}
