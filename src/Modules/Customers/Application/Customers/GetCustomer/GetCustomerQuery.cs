using Common.Application;
using Common.Domain;
using Modules.Customers.Domain.Customers;

namespace Modules.Customers.Application.Customers.GetCustomer;

public sealed record GetCustomerQuery(Guid CustomerId) : IQuery<CustomerDto>;

internal sealed class GetCustomerQueryHandler : IQueryHandler<GetCustomerQuery, CustomerDto>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerQueryHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(
            CustomerId.Create(request.CustomerId),
            cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerDto>(new Error(
                "Customer.NotFound",
                $"Customer with ID '{request.CustomerId}' was not found"));
        }

        var dto = new CustomerDto(
            customer.Id,
            customer.Name.FirstName,
            customer.Name.LastName,
            customer.Email,
            customer.PhoneNumber?.Value,
            customer.Status.ToString(),
            customer.Addresses.Select(a => new AddressDto(
                a.Street,
                a.City,
                a.State,
                a.Country,
                a.PostalCode)).ToList(),
            customer.CreatedAt,
            customer.UpdatedAt);

        return Result.Success(dto);
    }
}
