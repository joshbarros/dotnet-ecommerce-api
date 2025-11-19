using IntegrationTests.Fixtures;
using Modules.Catalog.Domain.Categories;
using Modules.Catalog.Domain.Common;
using Modules.Catalog.Domain.Products;
using Modules.Catalog.Infrastructure.Persistence;
using Modules.Catalog.Infrastructure.Persistence.Repositories;

namespace IntegrationTests.Infrastructure.Repositories;

public sealed class ProductRepositoryTests : IClassFixture<CatalogDbContextFixture>, IAsyncLifetime
{
    private readonly CatalogDbContextFixture _fixture;
    private CatalogDbContext _context = null!;
    private ProductRepository _repository = null!;
    private CategoryRepository _categoryRepository = null!;

    public ProductRepositoryTests(CatalogDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _context = _fixture.CreateDbContext();
        _repository = new ProductRepository(_context);
        _categoryRepository = new CategoryRepository(_context);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistProduct()
    {
        // Arrange
        var category = CreateTestCategory();
        await _categoryRepository.AddAsync(category);
        await _context.SaveChangesAsync();

        var product = CreateTestProduct(category.Id);

        // Act
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(product.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Value.Should().Be(product.Name.Value);
        retrieved.Price.Amount.Should().Be(product.Price.Amount);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        var category = CreateTestCategory();
        await _categoryRepository.AddAsync(category);
        await _context.SaveChangesAsync();

        var product = CreateTestProduct(category.Id);
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(product.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(product.Id);
        retrieved.Name.Value.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = ProductId.New();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeImages()
    {
        // Arrange
        var category = CreateTestCategory();
        await _categoryRepository.AddAsync(category);
        await _context.SaveChangesAsync();

        var product = CreateTestProduct(category.Id);
        product.AddImage("https://example.com/image1.jpg", 1);
        product.AddImage("https://example.com/image2.jpg", 2);
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(product.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Images.Should().HaveCount(2);
        retrieved.Images.First().Url.Should().Be("https://example.com/image1.jpg");
    }

    [Fact]
    public async Task Update_ShouldPersistChanges()
    {
        // Arrange
        var category = CreateTestCategory();
        await _categoryRepository.AddAsync(category);
        await _context.SaveChangesAsync();

        var product = CreateTestProduct(category.Id);
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var newPrice = Money.Create(199.99m, "USD");
        product.UpdatePrice(newPrice);
        _repository.Update(product);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(product.Id);
        retrieved!.Price.Amount.Should().Be(199.99m);
    }

    [Fact]
    public async Task Remove_ShouldDeleteProduct()
    {
        // Arrange
        var category = CreateTestCategory();
        await _categoryRepository.AddAsync(category);
        await _context.SaveChangesAsync();

        var product = CreateTestProduct(category.Id);
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(product);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(product.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnProductsInCategory()
    {
        // Arrange
        var category1 = CreateTestCategory();
        var category2 = Category.Create("Category 2", "category-2");
        await _categoryRepository.AddAsync(category1);
        await _categoryRepository.AddAsync(category2);
        await _context.SaveChangesAsync();

        var product1 = Product.Create(
            ProductName.Create("Product 1"),
            "Description 1",
            Money.Create(10m, "USD"),
            category1.Id);

        var product2 = Product.Create(
            ProductName.Create("Product 2"),
            "Description 2",
            Money.Create(20m, "USD"),
            category1.Id);

        var product3 = Product.Create(
            ProductName.Create("Product 3"),
            "Description 3",
            Money.Create(30m, "USD"),
            category2.Id);

        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.AddAsync(product3);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByCategoryAsync(category1.Id);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(p => p.Id == product1.Id);
        results.Should().Contain(p => p.Id == product2.Id);
        results.Should().NotContain(p => p.Id == product3.Id);
    }

    [Fact]
    public async Task ExistsAsync_WhenProductExists_ShouldReturnTrue()
    {
        // Arrange
        var category = CreateTestCategory();
        await _categoryRepository.AddAsync(category);
        await _context.SaveChangesAsync();

        var product = CreateTestProduct(category.Id);
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(product.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenProductDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = ProductId.New();

        // Act
        var exists = await _repository.ExistsAsync(nonExistentId);

        // Assert
        exists.Should().BeFalse();
    }

    private static Product CreateTestProduct(CategoryId categoryId)
    {
        return Product.Create(
            ProductName.Create("Test Product"),
            "Test description",
            Money.Create(99.99m, "USD"),
            categoryId);
    }

    private static Category CreateTestCategory()
    {
        return Category.Create("Test Category", "test-category");
    }
}
