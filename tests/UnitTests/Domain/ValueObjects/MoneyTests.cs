using Modules.Catalog.Domain.Common;

namespace UnitTests.Domain.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_WithValidValues_ShouldSucceed()
    {
        // Arrange & Act
        var money = Money.Create(100m, "USD");

        // Assert
        money.Amount.Should().Be(100m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowException()
    {
        // Act
        var act = () => Money.Create(-10m, "USD");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Amount cannot be negative");
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldThrowException()
    {
        // Act
        var act = () => Money.Create(100m, "");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Currency is required");
    }

    [Fact]
    public void Create_WithInvalidCurrencyLength_ShouldThrowException()
    {
        // Act
        var act = () => Money.Create(100m, "US");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Currency must be a 3-letter ISO code");
    }

    [Fact]
    public void Create_WithLowercaseCurrency_ShouldConvertToUppercase()
    {
        // Act
        var money = Money.Create(100m, "usd");

        // Assert
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSum()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "EUR");

        // Act
        var act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot add money with different currencies*");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnDifference()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(30m, "USD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Subtract_ResultingInNegative_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.Create(50m, "USD");
        var money2 = Money.Create(100m, "USD");

        // Act
        var act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Result cannot be negative");
    }

    [Fact]
    public void Multiply_WithPositiveMultiplier_ShouldReturnProduct()
    {
        // Arrange
        var money = Money.Create(50m, "USD");

        // Act
        var result = money.Multiply(3);

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Multiply_WithNegativeMultiplier_ShouldThrowException()
    {
        // Arrange
        var money = Money.Create(50m, "USD");

        // Act
        var act = () => money.Multiply(-2);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Multiplier cannot be negative");
    }

    [Fact]
    public void OperatorAdd_ShouldWork()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "USD");

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void OperatorMultiply_ShouldWork()
    {
        // Arrange
        var money = Money.Create(50m, "USD");

        // Act
        var result = money * 2;

        // Assert
        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(100m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentAmounts_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void Equals_WithDifferentCurrency_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(100m, "EUR");

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void FactoryMethods_ShouldCreateCorrectCurrency()
    {
        // Act
        var usd = Money.Usd(100m);
        var eur = Money.Eur(100m);
        var gbp = Money.Gbp(100m);

        // Assert
        usd.Currency.Should().Be("USD");
        eur.Currency.Should().Be("EUR");
        gbp.Currency.Should().Be("GBP");
    }
}
