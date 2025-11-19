using Common.Application;
using Common.Domain;
using Modules.Orders.Domain.Orders;

namespace Modules.Orders.Application.Orders.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IQuery<OrderDto>;

internal sealed class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderDto>
{
    private readonly IOrderRepository _repository;

    public GetOrderQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(
            OrderId.Create(request.OrderId),
            cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderDto>(new Error(
                "Order.NotFound",
                $"Order with ID '{request.OrderId}' was not found"));
        }

        var dto = new OrderDto(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            new AddressDto(
                order.ShippingAddress.Street,
                order.ShippingAddress.City,
                order.ShippingAddress.State,
                order.ShippingAddress.Country,
                order.ShippingAddress.PostalCode),
            order.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice.Amount,
                i.Subtotal.Amount,
                i.UnitPrice.Currency)).ToList(),
            order.CreatedAt,
            order.ConfirmedAt,
            order.ShippedAt,
            order.DeliveredAt,
            order.CancelledAt);

        return Result.Success(dto);
    }
}
