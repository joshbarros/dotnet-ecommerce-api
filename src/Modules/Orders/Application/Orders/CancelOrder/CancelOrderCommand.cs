using Common.Application;
using Common.Domain;
using FluentValidation;
using Modules.Orders.Application.Abstractions.Data;
using Modules.Orders.Domain.Orders;

namespace Modules.Orders.Application.Orders.CancelOrder;

public sealed record CancelOrderCommand(
    Guid OrderId,
    string Reason) : ICommand;

internal sealed class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrdersUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IOrdersUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
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

        var result = order.Cancel(request.Reason);

        if (result.IsFailure)
        {
            return result;
        }

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
    }
}
