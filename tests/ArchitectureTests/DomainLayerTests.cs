using NetArchTest.Rules;
using System.Reflection;

namespace ArchitectureTests;

/// <summary>
/// Architecture tests to ensure Domain layer purity and DDD patterns
/// </summary>
public sealed class DomainLayerTests
{
    private static readonly Assembly CatalogDomainAssembly = typeof(Modules.Catalog.Domain.Products.Product).Assembly;

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnEntityFramework()
    {
        // Arrange
        var domainTypes = Types.InAssembly(CatalogDomainAssembly);

        // Act
        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not have dependency on EF Core - it should be infrastructure concern");
    }

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnMediatR()
    {
        // Arrange
        var domainTypes = Types.InAssembly(CatalogDomainAssembly);

        // Act
        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOn("MediatR")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not have dependency on MediatR - it should be application concern");
    }

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnAspNetCore()
    {
        // Arrange
        var domainTypes = Types.InAssembly(CatalogDomainAssembly);

        // Act
        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not have dependency on ASP.NET Core");
    }

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnFluentValidation()
    {
        // Arrange
        var domainTypes = Types.InAssembly(CatalogDomainAssembly);

        // Act
        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOn("FluentValidation")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not have dependency on FluentValidation - validation should be in domain logic");
    }

    [Fact]
    public void Entities_ShouldBeSealed()
    {
        // Arrange
        var entityTypes = Types.InAssembly(CatalogDomainAssembly)
            .That()
            .Inherit(typeof(Common.Domain.Entity<>))
            .And()
            .AreNotAbstract();

        // Act
        var result = entityTypes
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All concrete entities should be sealed to prevent inheritance");
    }

    [Fact]
    public void AggregateRoots_ShouldBeSealed()
    {
        // Arrange
        var aggregateTypes = Types.InAssembly(CatalogDomainAssembly)
            .That()
            .Inherit(typeof(Common.Domain.AggregateRoot<>))
            .And()
            .AreNotAbstract();

        // Act
        var result = aggregateTypes
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All aggregate roots should be sealed to prevent inheritance");
    }

    [Fact]
    public void ValueObjects_ShouldBeSealed()
    {
        // Arrange
        var valueObjectTypes = Types.InAssembly(CatalogDomainAssembly)
            .That()
            .Inherit(typeof(Common.Domain.ValueObject))
            .And()
            .AreNotAbstract();

        // Act
        var result = valueObjectTypes
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All value objects should be sealed to prevent inheritance");
    }

    [Fact]
    public void DomainEvents_ShouldBeRecords()
    {
        // Arrange
        var domainEventTypes = Types.InAssembly(CatalogDomainAssembly)
            .That()
            .ImplementInterface(typeof(Common.Domain.IDomainEvent))
            .And()
            .AreNotAbstract();

        // Act
        var result = domainEventTypes
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All domain events should be sealed records for immutability");
    }

    [Fact]
    public void Repositories_ShouldBeInterfaces()
    {
        // Arrange
        var repositoryTypes = Types.InAssembly(CatalogDomainAssembly)
            .That()
            .HaveNameEndingWith("Repository");

        // Act
        var result = repositoryTypes
            .Should()
            .BeInterfaces()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All repositories in Domain should be interfaces");
    }

    [Fact]
    public void Repositories_ShouldResideInRepositoriesNamespace()
    {
        // Arrange
        var repositoryTypes = Types.InAssembly(CatalogDomainAssembly)
            .That()
            .HaveNameEndingWith("Repository");

        // Act
        var result = repositoryTypes
            .Should()
            .ResideInNamespace("Modules.Catalog.Domain.Products")
            .Or()
            .ResideInNamespace("Modules.Catalog.Domain.Categories")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Repositories should reside in their aggregate's namespace");
    }

    [Fact]
    public void DomainLayer_ShouldOnlyContainDomainConcerns()
    {
        // Arrange
        var domainTypes = Types.InAssembly(CatalogDomainAssembly);

        // Act
        var result = domainTypes
            .That()
            .ResideInNamespace("Modules.Catalog.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "System.Net.Http",
                "Newtonsoft.Json",
                "System.Text.Json")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not have dependencies on infrastructure concerns like HTTP or JSON");
    }
}
