using IntegrationTests.Fixtures;
using Modules.Catalog.Domain.Categories;
using Modules.Catalog.Infrastructure.Persistence;
using Modules.Catalog.Infrastructure.Persistence.Repositories;

namespace IntegrationTests.Infrastructure.Repositories;

public sealed class CategoryRepositoryTests : IClassFixture<CatalogDbContextFixture>, IAsyncLifetime
{
    private readonly CatalogDbContextFixture _fixture;
    private CatalogDbContext _context = null!;
    private CategoryRepository _repository = null!;

    public CategoryRepositoryTests(CatalogDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _context = _fixture.CreateDbContext();
        _repository = new CategoryRepository(_context);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistCategory()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");

        // Act
        await _repository.AddAsync(category);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(category.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Electronics");
        retrieved.Slug.Should().Be("electronics");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryExists_ShouldReturnCategory()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        await _repository.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(category.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = CategoryId.New();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_WhenSlugExists_ShouldReturnCategory()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        await _repository.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetBySlugAsync("electronics");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetBySlugAsync_WhenSlugDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetBySlugAsync("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SlugExistsAsync_WhenSlugExists_ShouldReturnTrue()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        await _repository.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.SlugExistsAsync("electronics");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task SlugExistsAsync_WhenSlugDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var exists = await _repository.SlugExistsAsync("non-existent");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetChildrenAsync_ShouldReturnChildCategories()
    {
        // Arrange
        var parent = Category.Create("Electronics", "electronics");
        await _repository.AddAsync(parent);
        await _context.SaveChangesAsync();

        var child1 = Category.Create("Smartphones", "smartphones", parent.Id);
        var child2 = Category.Create("Laptops", "laptops", parent.Id);
        var otherCategory = Category.Create("Clothing", "clothing");

        await _repository.AddAsync(child1);
        await _repository.AddAsync(child2);
        await _repository.AddAsync(otherCategory);
        await _context.SaveChangesAsync();

        // Act
        var children = await _repository.GetChildrenAsync(parent.Id);

        // Assert
        children.Should().HaveCount(2);
        children.Should().Contain(c => c.Id == child1.Id);
        children.Should().Contain(c => c.Id == child2.Id);
        children.Should().NotContain(c => c.Id == otherCategory.Id);
    }

    [Fact]
    public async Task GetChildrenAsync_WhenNoChildren_ShouldReturnEmpty()
    {
        // Arrange
        var parent = Category.Create("Electronics", "electronics");
        await _repository.AddAsync(parent);
        await _context.SaveChangesAsync();

        // Act
        var children = await _repository.GetChildrenAsync(parent.Id);

        // Assert
        children.Should().BeEmpty();
    }

    [Fact]
    public async Task Update_ShouldPersistChanges()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        await _repository.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        category.UpdateInformation("Consumer Electronics", "Updated description");
        _repository.Update(category);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(category.Id);
        retrieved!.Name.Should().Be("Consumer Electronics");
        retrieved.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task Remove_ShouldDeleteCategory()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        await _repository.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(category);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(category.Id);
        retrieved.Should().BeNull();
    }
}
