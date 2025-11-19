using Modules.Catalog.Domain.Products;

namespace UnitTests.Domain.ValueObjects;

public sealed class ProductNameTests
{
    [Fact]
    public void Create_WithValidName_ShouldSucceed()
    {
        // Arrange & Act
        var name = ProductName.Create("iPhone 15 Pro");

        // Assert
        name.Value.Should().Be("iPhone 15 Pro");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowException()
    {
        // Act
        var act = () => ProductName.Create("");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Product name cannot be empty");
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowException()
    {
        // Act
        var act = () => ProductName.Create("   ");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Product name cannot be empty");
    }

    [Fact]
    public void Create_WithNameExceeding200Characters_ShouldThrowException()
    {
        // Arrange
        var longName = new string('a', 201);

        // Act
        var act = () => ProductName.Create(longName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Product name cannot exceed 200 characters");
    }

    [Fact]
    public void Create_WithNameContainingLeadingTrailingSpaces_ShouldTrim()
    {
        // Act
        var name = ProductName.Create("  iPhone 15 Pro  ");

        // Assert
        name.Value.Should().Be("iPhone 15 Pro");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var name = ProductName.Create("Test Product");

        // Act
        string value = name;

        // Assert
        value.Should().Be("Test Product");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var name = ProductName.Create("Test Product");

        // Act
        var result = name.ToString();

        // Assert
        result.Should().Be("Test Product");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var name1 = ProductName.Create("Test Product");
        var name2 = ProductName.Create("Test Product");

        // Act & Assert
        name1.Should().Be(name2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var name1 = ProductName.Create("Product A");
        var name2 = ProductName.Create("Product B");

        // Act & Assert
        name1.Should().NotBe(name2);
    }
}
