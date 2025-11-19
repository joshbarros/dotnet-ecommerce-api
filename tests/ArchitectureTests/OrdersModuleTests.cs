using NetArchTest.Rules;
using System.Reflection;

namespace ArchitectureTests;

/// <summary>
/// Architecture tests for Orders module
/// </summary>
public sealed class OrdersModuleTests
{
    private static readonly Assembly OrdersDomainAssembly = typeof(Modules.Orders.Domain.Orders.Order).Assembly;
    private static readonly Assembly OrdersApplicationAssembly = typeof(Modules.Orders.Application.AssemblyReference).Assembly;
    private static readonly Assembly OrdersInfrastructureAssembly = typeof(Modules.Orders.Infrastructure.AssemblyReference).Assembly;

    [Fact]
    public void OrdersDomain_ShouldNotDependOnApplication()
    {
        var result = Types.InAssembly(OrdersDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Modules.Orders.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Orders Domain layer should not depend on Application layer");
    }

    [Fact]
    public void OrdersDomain_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(OrdersDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Modules.Orders.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Orders Domain layer should not depend on Infrastructure layer");
    }

    [Fact]
    public void OrdersApplication_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(OrdersApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Modules.Orders.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Orders Application layer should not depend on Infrastructure layer");
    }

    [Fact]
    public void OrdersDomain_CanDependOnCustomersDomain()
    {
        var result = Types.InAssembly(OrdersDomainAssembly)
            .That()
            .ResideInNamespace("Modules.Orders.Domain")
            .Should()
            .HaveDependencyOn("Modules.Customers.Domain")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Orders Domain can depend on Customers Domain for cross-module references");
    }

    [Fact]
    public void OrdersDomain_CanDependOnCatalogDomain()
    {
        var result = Types.InAssembly(OrdersDomainAssembly)
            .That()
            .ResideInNamespace("Modules.Orders.Domain")
            .Should()
            .HaveDependencyOn("Modules.Catalog.Domain")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Orders Domain can depend on Catalog Domain for cross-module references");
    }

    [Fact]
    public void OrdersModule_CommandsShouldBeSealed()
    {
        var commandTypes = Types.InAssembly(OrdersApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.ICommand))
            .Or()
            .ImplementInterface(typeof(Common.Application.ICommand<>));

        var result = commandTypes
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "All commands in Orders module should be sealed");
    }

    [Fact]
    public void OrdersModule_QueriesShouldBeSealed()
    {
        var queryTypes = Types.InAssembly(OrdersApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.IQuery<>));

        var result = queryTypes
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "All queries in Orders module should be sealed");
    }

    [Fact]
    public void OrdersModule_AggregatesShouldBeSealed()
    {
        var aggregateTypes = Types.InAssembly(OrdersDomainAssembly)
            .That()
            .Inherit(typeof(Common.Domain.AggregateRoot<>))
            .And()
            .AreNotAbstract();

        var result = aggregateTypes
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "All aggregates in Orders module should be sealed");
    }

    [Fact]
    public void OrdersModule_ShouldNotDependOnPresentation()
    {
        var result = Types.InAssembly(OrdersDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Modules.Orders.Presentation")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Orders Domain should not depend on Presentation layer");
    }
}
