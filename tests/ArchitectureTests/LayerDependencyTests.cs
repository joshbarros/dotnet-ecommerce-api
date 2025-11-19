using NetArchTest.Rules;
using System.Reflection;

namespace ArchitectureTests;

/// <summary>
/// Architecture tests to enforce Clean Architecture layer dependencies
/// </summary>
public sealed class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(Common.Domain.Entity<>).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Common.Application.ICommand<>).Assembly;
    private static readonly Assembly CatalogDomainAssembly = typeof(Modules.Catalog.Domain.Products.Product).Assembly;
    private static readonly Assembly CatalogApplicationAssembly = typeof(Modules.Catalog.Application.AssemblyReference).Assembly;
    private static readonly Assembly CatalogInfrastructureAssembly = typeof(Modules.Catalog.Infrastructure.AssemblyReference).Assembly;

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnApplication()
    {
        // Arrange
        var domainTypes = Types.InAssembly(CatalogDomainAssembly);

        // Act
        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOn("Modules.Catalog.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on Application layer");
    }

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnInfrastructure()
    {
        // Arrange
        var domainTypes = Types.InAssembly(CatalogDomainAssembly);

        // Act
        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOn("Modules.Catalog.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on Infrastructure layer");
    }

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnPresentation()
    {
        // Arrange
        var domainTypes = Types.InAssembly(CatalogDomainAssembly);

        // Act
        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOn("Modules.Catalog.Presentation")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on Presentation layer");
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOnInfrastructure()
    {
        // Arrange
        var applicationTypes = Types.InAssembly(CatalogApplicationAssembly);

        // Act
        var result = applicationTypes
            .ShouldNot()
            .HaveDependencyOn("Modules.Catalog.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should not depend on Infrastructure layer");
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOnPresentation()
    {
        // Arrange
        var applicationTypes = Types.InAssembly(CatalogApplicationAssembly);

        // Act
        var result = applicationTypes
            .ShouldNot()
            .HaveDependencyOn("Modules.Catalog.Presentation")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should not depend on Presentation layer");
    }

    [Fact]
    public void Infrastructure_ShouldNotHaveDependencyOnPresentation()
    {
        // Arrange
        var infrastructureTypes = Types.InAssembly(CatalogInfrastructureAssembly);

        // Act
        var result = infrastructureTypes
            .ShouldNot()
            .HaveDependencyOn("Modules.Catalog.Presentation")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Infrastructure layer should not depend on Presentation layer");
    }

    [Fact]
    public void Application_CanDependOnDomain()
    {
        // Arrange
        var applicationTypes = Types.InAssembly(CatalogApplicationAssembly);

        // Act
        var result = applicationTypes
            .That()
            .ResideInNamespace("Modules.Catalog.Application")
            .Should()
            .HaveDependencyOn("Modules.Catalog.Domain")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should depend on Domain layer");
    }

    [Fact]
    public void Infrastructure_CanDependOnDomain()
    {
        // Arrange
        var infrastructureTypes = Types.InAssembly(CatalogInfrastructureAssembly);

        // Act
        var result = infrastructureTypes
            .That()
            .ResideInNamespace("Modules.Catalog.Infrastructure")
            .Should()
            .HaveDependencyOn("Modules.Catalog.Domain")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Infrastructure layer should depend on Domain layer");
    }

    [Fact]
    public void Infrastructure_CanDependOnApplication()
    {
        // Arrange
        var infrastructureTypes = Types.InAssembly(CatalogInfrastructureAssembly);

        // Act
        var result = infrastructureTypes
            .That()
            .ResideInNamespace("Modules.Catalog.Infrastructure")
            .Should()
            .HaveDependencyOn("Modules.Catalog.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Infrastructure layer should depend on Application layer for interfaces");
    }
}
