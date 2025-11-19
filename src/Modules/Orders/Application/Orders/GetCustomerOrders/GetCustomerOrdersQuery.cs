using Common.Application;
using Common.Domain;
using Modules.Customers.Domain.Customers;
using Modules.Orders.Domain.Orders;

namespace Modules.Orders.Application.Orders.GetCustomerOrders;

public sealed record GetCustomerOrdersQuery(Guid CustomerId) : IQuery<List<OrderDto>>;

internal sealed class GetCustomerOrdersQueryHandler : IQueryHandler<GetCustomerOrdersQuery, List<OrderDto>>
{
    private readonly IOrderRepository _repository;

    public GetCustomerOrdersQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<OrderDto>>> Handle(
        GetCustomerOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var customerId = CustomerId.Create(request.CustomerId);

        var orders = await _repository.GetByCustomerIdAsync(customerId, cancellationToken);

        var dtos = orders.Select(order => new OrderDto(
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
            order.CancelledAt)).ToList();

        return Result.Success(dtos);
    }
}
