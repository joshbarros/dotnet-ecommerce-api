using Common.Domain;
using System.Text.RegularExpressions;

namespace Modules.Customers.Domain.Common;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; private init; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email cannot be empty", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 254)
        {
            throw new ArgumentException("Email is too long (max 254 characters)", nameof(value));
        }

        if (!EmailRegex.IsMatch(normalized))
        {
            throw new ArgumentException($"Email '{value}' is not in a valid format", nameof(value));
        }

        return new Email(normalized);
    }

    public static implicit operator string(Email email) => email.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
