using Common.Application;
using Common.Domain;
using Modules.Customers.Domain.Customers;

namespace Modules.Customers.Application.Customers.GetAllCustomers;

public sealed record GetAllCustomersQuery : IQuery<List<CustomerDto>>;

internal sealed class GetAllCustomersQueryHandler : IQueryHandler<GetAllCustomersQuery, List<CustomerDto>>
{
    private readonly ICustomerRepository _repository;

    public GetAllCustomersQueryHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<CustomerDto>>> Handle(
        GetAllCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var customers = await _repository.GetAllAsync(cancellationToken);

        var dtos = customers.Select(customer => new CustomerDto(
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
            customer.UpdatedAt)).ToList();

        return Result.Success(dtos);
    }
}
