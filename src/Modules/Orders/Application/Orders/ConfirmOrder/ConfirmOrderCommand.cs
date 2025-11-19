using Common.Application;
using Common.Domain;
using FluentValidation;
using Modules.Orders.Application.Abstractions.Data;
using Modules.Orders.Domain.Orders;

namespace Modules.Orders.Application.Orders.ConfirmOrder;

public sealed record ConfirmOrderCommand(Guid OrderId) : ICommand;

internal sealed class ConfirmOrderCommandHandler : ICommandHandler<ConfirmOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrdersUnitOfWork _unitOfWork;

    public ConfirmOrderCommandHandler(
        IOrderRepository orderRepository,
        IOrdersUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
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

        var result = order.Confirm();

        if (result.IsFailure)
        {
            return result;
        }

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
{
    public ConfirmOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");
    }
}
