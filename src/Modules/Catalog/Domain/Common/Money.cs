using Common.Domain;

namespace Modules.Catalog.Domain.Common;

/// <summary>
/// Value object representing monetary value
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; private init; }
    public string Currency { get; private init; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new DomainException("Amount cannot be negative");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required");
        }

        if (currency.Length != 3)
        {
            throw new DomainException("Currency must be a 3-letter ISO code");
        }

        return new Money(amount, currency.ToUpperInvariant());
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new DomainException(
                $"Cannot add money with different currencies: {Currency} and {other.Currency}");
        }

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new DomainException(
                $"Cannot subtract money with different currencies: {Currency} and {other.Currency}");
        }

        var result = Amount - other.Amount;
        if (result < 0)
        {
            throw new DomainException("Result cannot be negative");
        }

        return new Money(result, Currency);
    }

    public Money Multiply(decimal multiplier)
    {
        if (multiplier < 0)
        {
            throw new DomainException("Multiplier cannot be negative");
        }

        return new Money(Amount * multiplier, Currency);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal multiplier) => money.Multiply(multiplier);
    public static Money operator *(decimal multiplier, Money money) => money.Multiply(multiplier);

    // Factory methods for common currencies
    public static Money Usd(decimal amount) => Create(amount, "USD");
    public static Money Eur(decimal amount) => Create(amount, "EUR");
    public static Money Gbp(decimal amount) => Create(amount, "GBP");
}
