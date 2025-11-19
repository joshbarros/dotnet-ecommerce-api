using Common.Application;
using Common.Domain;
using FluentValidation;
using Modules.Customers.Domain.Common;
using Modules.Customers.Domain.Customers;
using Modules.Orders.Application.Abstractions.Data;
using Modules.Orders.Domain.Orders;

namespace Modules.Orders.Application.Orders.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode) : ICommand<Guid>;

internal sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrdersUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IOrdersUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customerId = CustomerId.Create(request.CustomerId);

        var shippingAddress = Address.Create(
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.PostalCode);

        var order = Order.Create(customerId, shippingAddress);

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success((Guid)order.Id);
    }
}

internal sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required")
            .MaximumLength(200).WithMessage("Street must not exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required")
            .MaximumLength(100).WithMessage("State must not exceed 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");
    }
}
