using Common.Domain;

namespace Modules.Customers.Domain.Common;

public sealed class CustomerName : ValueObject
{
    public string FirstName { get; private init; }
    public string LastName { get; private init; }

    private CustomerName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static CustomerName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));
        }

        var trimmedFirstName = firstName.Trim();
        var trimmedLastName = lastName.Trim();

        if (trimmedFirstName.Length < 2 || trimmedFirstName.Length > 50)
        {
            throw new ArgumentException("First name must be between 2 and 50 characters", nameof(firstName));
        }

        if (trimmedLastName.Length < 2 || trimmedLastName.Length > 50)
        {
            throw new ArgumentException("Last name must be between 2 and 50 characters", nameof(lastName));
        }

        return new CustomerName(trimmedFirstName, trimmedLastName);
    }

    public string FullName => $"{FirstName} {LastName}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => FullName;
}
