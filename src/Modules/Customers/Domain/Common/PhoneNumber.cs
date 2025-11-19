using Common.Domain;
using System.Text.RegularExpressions;

namespace Modules.Customers.Domain.Common;

public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    public string Value { get; private init; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Phone number cannot be empty", nameof(value));
        }

        // Remove common formatting characters
        var cleaned = value.Replace(" ", "")
                          .Replace("-", "")
                          .Replace("(", "")
                          .Replace(")", "")
                          .Replace(".", "");

        if (!PhoneRegex.IsMatch(cleaned))
        {
            throw new ArgumentException($"Phone number '{value}' is not in a valid format", nameof(value));
        }

        return new PhoneNumber(cleaned);
    }

    public static implicit operator string(PhoneNumber phone) => phone.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
