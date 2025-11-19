using Modules.Catalog.Domain.Categories;

namespace UnitTests.Domain.CategoryTests;

public sealed class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Act
        var category = Category.Create("Electronics", "electronics");

        // Assert
        category.Should().NotBeNull();
        category.Name.Should().Be("Electronics");
        category.Slug.Should().Be("electronics");
        category.ParentId.Should().BeNull();
    }

    [Fact]
    public void Create_WithParentId_ShouldSucceed()
    {
        // Arrange
        var parentId = CategoryId.New();

        // Act
        var category = Category.Create("Smartphones", "smartphones", parentId);

        // Assert
        category.ParentId.Should().Be(parentId);
    }

    [Fact]
    public void Create_ShouldRaiseCategoryCreatedEvent()
    {
        // Act
        var category = Category.Create("Electronics", "electronics");

        // Assert
        category.DomainEvents.Should().HaveCount(1);
        var domainEvent = category.DomainEvents.First();
        domainEvent.Should().BeOfType<CategoryCreatedEvent>();

        var categoryCreatedEvent = (CategoryCreatedEvent)domainEvent;
        categoryCreatedEvent.CategoryId.Should().Be(category.Id.Value);
        categoryCreatedEvent.Name.Should().Be("Electronics");
        categoryCreatedEvent.Slug.Should().Be("electronics");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowException()
    {
        // Act
        var act = () => Category.Create("", "electronics");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Category name cannot be empty");
    }

    [Fact]
    public void Create_WithEmptySlug_ShouldThrowException()
    {
        // Act
        var act = () => Category.Create("Electronics", "");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Category slug cannot be empty");
    }

    [Fact]
    public void Create_WithNameExceeding100Characters_ShouldThrowException()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act
        var act = () => Category.Create(longName, "slug");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Category name cannot exceed 100 characters");
    }

    [Fact]
    public void Create_ShouldConvertSlugToLowercase()
    {
        // Act
        var category = Category.Create("Electronics", "ELECTRONICS");

        // Assert
        category.Slug.Should().Be("electronics");
    }

    [Fact]
    public void Create_ShouldTrimCategoryName()
    {
        // Act
        var category = Category.Create("  Electronics  ", "electronics");

        // Assert
        category.Name.Should().Be("Electronics");
    }

    [Fact]
    public void UpdateInformation_WithValidData_ShouldSucceed()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        category.ClearDomainEvents();

        // Act
        var result = category.UpdateInformation("Consumer Electronics", "All electronic devices");

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be("Consumer Electronics");
        category.Description.Should().Be("All electronic devices");
    }

    [Fact]
    public void UpdateInformation_ShouldRaiseCategoryUpdatedEvent()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        category.ClearDomainEvents();

        // Act
        category.UpdateInformation("Consumer Electronics", "Description");

        // Assert
        category.DomainEvents.Should().HaveCount(1);
        var domainEvent = category.DomainEvents.First();
        domainEvent.Should().BeOfType<CategoryUpdatedEvent>();
    }

    [Fact]
    public void UpdateInformation_WithEmptyName_ShouldFail()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");

        // Act
        var result = category.UpdateInformation("", "Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.InvalidName");
    }

    [Fact]
    public void UpdateInformation_WithNameExceeding100Characters_ShouldFail()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        var longName = new string('a', 101);

        // Act
        var result = category.UpdateInformation(longName, "Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NameTooLong");
    }

    [Fact]
    public void UpdateInformation_ShouldTrimName()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");

        // Act
        var result = category.UpdateInformation("  Consumer Electronics  ", null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be("Consumer Electronics");
    }
}
