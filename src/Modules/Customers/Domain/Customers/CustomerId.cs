namespace Modules.Customers.Domain.Customers;

public sealed record CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.NewGuid());

    public static CustomerId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty", nameof(value));
        }

        return new CustomerId(value);
    }

    public static implicit operator Guid(CustomerId customerId) => customerId.Value;

    public override string ToString() => Value.ToString();
}
