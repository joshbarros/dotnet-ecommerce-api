using NetArchTest.Rules;
using System.Reflection;

namespace ArchitectureTests;

/// <summary>
/// Architecture tests for Customers module
/// </summary>
public sealed class CustomersModuleTests
{
    private static readonly Assembly CustomersDomainAssembly = typeof(Modules.Customers.Domain.Customers.Customer).Assembly;
    private static readonly Assembly CustomersApplicationAssembly = typeof(Modules.Customers.Application.AssemblyReference).Assembly;
    private static readonly Assembly CustomersInfrastructureAssembly = typeof(Modules.Customers.Infrastructure.AssemblyReference).Assembly;

    [Fact]
    public void CustomersDomain_ShouldNotDependOnApplication()
    {
        var result = Types.InAssembly(CustomersDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Modules.Customers.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Customers Domain layer should not depend on Application layer");
    }

    [Fact]
    public void CustomersDomain_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(CustomersDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Modules.Customers.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Customers Domain layer should not depend on Infrastructure layer");
    }

    [Fact]
    public void CustomersApplication_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(CustomersApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Modules.Customers.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Customers Application layer should not depend on Infrastructure layer");
    }

    [Fact]
    public void CustomersModule_CommandsShouldBeSealed()
    {
        var commandTypes = Types.InAssembly(CustomersApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.ICommand))
            .Or()
            .ImplementInterface(typeof(Common.Application.ICommand<>));

        var result = commandTypes
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "All commands in Customers module should be sealed");
    }

    [Fact]
    public void CustomersModule_QueriesShouldBeSealed()
    {
        var queryTypes = Types.InAssembly(CustomersApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.IQuery<>));

        var result = queryTypes
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "All queries in Customers module should be sealed");
    }

    [Fact]
    public void CustomersModule_AggregatesShouldBeSealed()
    {
        var aggregateTypes = Types.InAssembly(CustomersDomainAssembly)
            .That()
            .Inherit(typeof(Common.Domain.AggregateRoot<>))
            .And()
            .AreNotAbstract();

        var result = aggregateTypes
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "All aggregates in Customers module should be sealed");
    }

    [Fact]
    public void CustomersModule_ValueObjectsShouldBeSealed()
    {
        var valueObjectTypes = Types.InAssembly(CustomersDomainAssembly)
            .That()
            .Inherit(typeof(Common.Domain.ValueObject))
            .And()
            .AreNotAbstract();

        var result = valueObjectTypes
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "All value objects in Customers module should be sealed");
    }
}
