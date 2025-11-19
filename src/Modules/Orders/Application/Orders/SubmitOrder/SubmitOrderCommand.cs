using Common.Application;
using Common.Domain;
using FluentValidation;
using Modules.Orders.Application.Abstractions.Data;
using Modules.Orders.Domain.Orders;

namespace Modules.Orders.Application.Orders.SubmitOrder;

public sealed record SubmitOrderCommand(Guid OrderId) : ICommand;

internal sealed class SubmitOrderCommandHandler : ICommandHandler<SubmitOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrdersUnitOfWork _unitOfWork;

    public SubmitOrderCommandHandler(
        IOrderRepository orderRepository,
        IOrdersUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SubmitOrderCommand request, CancellationToken cancellationToken)
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

        var result = order.Submit();

        if (result.IsFailure)
        {
            return result;
        }

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class SubmitOrderCommandValidator : AbstractValidator<SubmitOrderCommand>
{
    public SubmitOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");
    }
}
