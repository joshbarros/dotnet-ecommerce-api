namespace Modules.Customers.Application.Customers;

public sealed record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Status,
    List<AddressDto> Addresses,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record AddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode);
