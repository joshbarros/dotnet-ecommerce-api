using Modules.Catalog.Domain.Categories;
using Modules.Catalog.Domain.Common;
using Modules.Catalog.Domain.Products;

namespace UnitTests.Domain.ProductTests;

public sealed class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var name = ProductName.Create("iPhone 15 Pro");
        var price = Money.Create(999m, "USD");
        var categoryId = CategoryId.New();

        // Act
        var product = Product.Create(name, "Latest iPhone", price, categoryId);

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be(name);
        product.Description.Should().Be("Latest iPhone");
        product.Price.Should().Be(price);
        product.CategoryId.Should().Be(categoryId);
        product.Status.Should().Be(ProductStatus.Draft);
        product.Images.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldRaiseProductCreatedEvent()
    {
        // Arrange
        var name = ProductName.Create("iPhone 15 Pro");
        var price = Money.Create(999m, "USD");
        var categoryId = CategoryId.New();

        // Act
        var product = Product.Create(name, "Latest iPhone", price, categoryId);

        // Assert
        product.DomainEvents.Should().HaveCount(1);
        var domainEvent = product.DomainEvents.First();
        domainEvent.Should().BeOfType<ProductCreatedEvent>();

        var productCreatedEvent = (ProductCreatedEvent)domainEvent;
        productCreatedEvent.ProductId.Should().Be(product.Id.Value);
        productCreatedEvent.Name.Should().Be("iPhone 15 Pro");
        productCreatedEvent.Price.Should().Be(999m);
        productCreatedEvent.Currency.Should().Be("USD");
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_ShouldSucceed()
    {
        // Arrange
        var product = CreateTestProduct();
        var newPrice = Money.Create(1099m, "USD");

        // Act
        var result = product.UpdatePrice(newPrice);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Price.Should().Be(newPrice);
    }

    [Fact]
    public void UpdatePrice_ShouldRaiseProductPriceChangedEvent()
    {
        // Arrange
        var product = CreateTestProduct();
        product.ClearDomainEvents(); // Clear creation event
        var newPrice = Money.Create(1099m, "USD");

        // Act
        product.UpdatePrice(newPrice);

        // Assert
        product.DomainEvents.Should().HaveCount(1);
        var domainEvent = product.DomainEvents.First();
        domainEvent.Should().BeOfType<ProductPriceChangedEvent>();
    }

    [Fact]
    public void UpdatePrice_WhenProductIsDiscontinued_ShouldFail()
    {
        // Arrange
        var product = CreateTestProduct();
        product.Discontinue();

        // Act
        var result = product.UpdatePrice(Money.Create(1099m, "USD"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.Discontinued");
    }

    [Fact]
    public void UpdateInformation_WithValidData_ShouldSucceed()
    {
        // Arrange
        var product = CreateTestProduct();
        var newName = ProductName.Create("iPhone 15 Pro Max");
        var newDescription = "Updated description";

        // Act
        var result = product.UpdateInformation(newName, newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Name.Should().Be(newName);
        product.Description.Should().Be(newDescription);
    }

    [Fact]
    public void UpdateInformation_WhenProductIsDiscontinued_ShouldFail()
    {
        // Arrange
        var product = CreateTestProduct();
        product.Discontinue();

        // Act
        var result = product.UpdateInformation(
            ProductName.Create("New Name"),
            "New description");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddImage_WithValidUrl_ShouldSucceed()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act
        var result = product.AddImage("https://example.com/image1.jpg", 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Images.Should().HaveCount(1);
        product.Images.First().Url.Should().Be("https://example.com/image1.jpg");
        product.Images.First().DisplayOrder.Should().Be(1);
    }

    [Fact]
    public void AddImage_WithEmptyUrl_ShouldFail()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act
        var result = product.AddImage("", 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.InvalidImage");
    }

    [Fact]
    public void AddImage_WhenExceedingMaximum_ShouldFail()
    {
        // Arrange
        var product = CreateTestProduct();
        for (int i = 1; i <= 10; i++)
        {
            product.AddImage($"https://example.com/image{i}.jpg", i);
        }

        // Act
        var result = product.AddImage("https://example.com/image11.jpg", 11);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.TooManyImages");
        product.Images.Should().HaveCount(10);
    }

    [Fact]
    public void Activate_WithImages_ShouldSucceed()
    {
        // Arrange
        var product = CreateTestProduct();
        product.AddImage("https://example.com/image1.jpg", 1);

        // Act
        var result = product.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public void Activate_WithoutImages_ShouldFail()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act
        var result = product.Activate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NoImages");
    }

    [Fact]
    public void Activate_WhenAlreadyDiscontinued_ShouldFail()
    {
        // Arrange
        var product = CreateTestProduct();
        product.AddImage("https://example.com/image1.jpg", 1);
        product.Discontinue();

        // Act
        var result = product.Activate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.Discontinued");
    }

    [Fact]
    public void MarkAsOutOfStock_WhenActive_ShouldSucceed()
    {
        // Arrange
        var product = CreateTestProduct();
        product.AddImage("https://example.com/image1.jpg", 1);
        product.Activate();

        // Act
        var result = product.MarkAsOutOfStock();

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.OutOfStock);
    }

    [Fact]
    public void MarkAsOutOfStock_WhenDiscontinued_ShouldFail()
    {
        // Arrange
        var product = CreateTestProduct();
        product.Discontinue();

        // Act
        var result = product.MarkAsOutOfStock();

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Discontinue_ShouldChangeStatusAndRaiseEvent()
    {
        // Arrange
        var product = CreateTestProduct();
        product.ClearDomainEvents();

        // Act
        var result = product.Discontinue();

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Discontinued);
        product.DomainEvents.Should().HaveCount(1);
        product.DomainEvents.First().Should().BeOfType<ProductDiscontinuedEvent>();
    }

    [Fact]
    public void Discontinue_WhenAlreadyDiscontinued_ShouldBeIdempotent()
    {
        // Arrange
        var product = CreateTestProduct();
        product.Discontinue();

        // Act
        var result = product.Discontinue();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    private static Product CreateTestProduct()
    {
        return Product.Create(
            ProductName.Create("Test Product"),
            "Test description",
            Money.Create(99.99m, "USD"),
            CategoryId.New());
    }
}
