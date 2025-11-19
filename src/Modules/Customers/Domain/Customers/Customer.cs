using Common.Domain;
using Modules.Customers.Domain.Common;
using Modules.Customers.Domain.Customers.Events;

namespace Modules.Customers.Domain.Customers;

public sealed class Customer : AggregateRoot<CustomerId>
{
    private readonly List<Address> _addresses = new();

    public CustomerName Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public PhoneNumber? PhoneNumber { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<Address> Addresses => _addresses.AsReadOnly();

    private Customer() { }

    public static Customer Create(CustomerName name, Email email, PhoneNumber? phoneNumber = null)
    {
        var customer = new Customer
        {
            Id = CustomerId.New(),
            Name = name,
            Email = email,
            PhoneNumber = phoneNumber,
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        customer.RaiseDomainEvent(new CustomerCreatedEvent(customer.Id, name.FullName, email));

        return customer;
    }

    public Result UpdateContactInfo(CustomerName name, Email email, PhoneNumber? phoneNumber)
    {
        if (Status == CustomerStatus.Deleted)
        {
            return Result.Failure(new Error(
                "Customer.Deleted",
                "Cannot update deleted customer"));
        }

        var oldEmail = Email;

        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;

        if (oldEmail.Value != email.Value)
        {
            RaiseDomainEvent(new CustomerEmailChangedEvent(Id, oldEmail, email));
        }

        return Result.Success();
    }

    public Result AddAddress(Address address)
    {
        if (Status == CustomerStatus.Deleted)
        {
            return Result.Failure(new Error(
                "Customer.Deleted",
                "Cannot add address to deleted customer"));
        }

        if (_addresses.Any(a => a == address))
        {
            return Result.Failure(new Error(
                "Customer.DuplicateAddress",
                "This address already exists for the customer"));
        }

        _addresses.Add(address);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result RemoveAddress(Address address)
    {
        if (Status == CustomerStatus.Deleted)
        {
            return Result.Failure(new Error(
                "Customer.Deleted",
                "Cannot remove address from deleted customer"));
        }

        var removed = _addresses.Remove(address);

        if (!removed)
        {
            return Result.Failure(new Error(
                "Customer.AddressNotFound",
                "Address not found"));
        }

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Suspend(string reason)
    {
        if (Status == CustomerStatus.Deleted)
        {
            return Result.Failure(new Error(
                "Customer.Deleted",
                "Cannot suspend deleted customer"));
        }

        if (Status == CustomerStatus.Suspended)
        {
            return Result.Failure(new Error(
                "Customer.AlreadySuspended",
                "Customer is already suspended"));
        }

        Status = CustomerStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CustomerSuspendedEvent(Id, reason));

        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == CustomerStatus.Deleted)
        {
            return Result.Failure(new Error(
                "Customer.Deleted",
                "Cannot activate deleted customer"));
        }

        if (Status == CustomerStatus.Active)
        {
            return Result.Failure(new Error(
                "Customer.AlreadyActive",
                "Customer is already active"));
        }

        Status = CustomerStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CustomerActivatedEvent(Id));

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (Status == CustomerStatus.Deleted)
        {
            return Result.Failure(new Error(
                "Customer.Deleted",
                "Cannot deactivate deleted customer"));
        }

        if (Status == CustomerStatus.Inactive)
        {
            return Result.Failure(new Error(
                "Customer.AlreadyInactive",
                "Customer is already inactive"));
        }

        Status = CustomerStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Delete()
    {
        if (Status == CustomerStatus.Deleted)
        {
            return Result.Failure(new Error(
                "Customer.AlreadyDeleted",
                "Customer is already deleted"));
        }

        Status = CustomerStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CustomerDeletedEvent(Id));

        return Result.Success();
    }
}
