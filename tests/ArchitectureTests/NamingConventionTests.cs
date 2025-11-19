using NetArchTest.Rules;
using System.Reflection;

namespace ArchitectureTests;

/// <summary>
/// Architecture tests to enforce naming conventions
/// </summary>
public sealed class NamingConventionTests
{
    private static readonly Assembly CatalogDomainAssembly = typeof(Modules.Catalog.Domain.Products.Product).Assembly;
    private static readonly Assembly CatalogApplicationAssembly = typeof(Modules.Catalog.Application.AssemblyReference).Assembly;
    private static readonly Assembly CatalogInfrastructureAssembly = typeof(Modules.Catalog.Infrastructure.AssemblyReference).Assembly;

    [Fact]
    public void Interfaces_ShouldStartWithI()
    {
        // Arrange
        var assemblies = new[]
        {
            CatalogDomainAssembly,
            CatalogApplicationAssembly,
            CatalogInfrastructureAssembly
        };

        foreach (var assembly in assemblies)
        {
            var interfaces = Types.InAssembly(assembly)
                .That()
                .AreInterfaces();

            // Act
            var result = interfaces
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue(
                $"All interfaces in {assembly.GetName().Name} should start with 'I'");
        }
    }

    [Fact]
    public void Commands_ShouldEndWithCommand()
    {
        // Arrange
        var commandTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.ICommand))
            .Or()
            .ImplementInterface(typeof(Common.Application.ICommand<>));

        // Act
        var result = commandTypes
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All commands should end with 'Command' suffix");
    }

    [Fact]
    public void CommandHandlers_ShouldEndWithCommandHandler()
    {
        // Arrange
        var commandHandlerTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(Common.Application.ICommandHandler<,>));

        // Act
        var result = commandHandlerTypes
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All command handlers should end with 'CommandHandler' suffix");
    }

    [Fact]
    public void Queries_ShouldEndWithQuery()
    {
        // Arrange
        var queryTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.IQuery<>));

        // Act
        var result = queryTypes
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All queries should end with 'Query' suffix");
    }

    [Fact]
    public void QueryHandlers_ShouldEndWithQueryHandler()
    {
        // Arrange
        var queryHandlerTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.IQueryHandler<,>));

        // Act
        var result = queryHandlerTypes
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All query handlers should end with 'QueryHandler' suffix");
    }

    [Fact]
    public void Validators_ShouldEndWithValidator()
    {
        // Arrange
        var validatorTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>));

        // Act
        var result = validatorTypes
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All validators should end with 'Validator' suffix");
    }

    [Fact]
    public void Repositories_ShouldEndWithRepository()
    {
        // Arrange
        var repositoryTypes = Types.InAssembly(CatalogInfrastructureAssembly)
            .That()
            .ResideInNamespace("Modules.Catalog.Infrastructure.Persistence.Repositories")
            .And()
            .AreClasses();

        // Act
        var result = repositoryTypes
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All repository implementations should end with 'Repository' suffix");
    }

    [Fact]
    public void DomainEvents_ShouldEndWithEvent()
    {
        // Arrange
        var domainEventTypes = Types.InAssembly(CatalogDomainAssembly)
            .That()
            .ImplementInterface(typeof(Common.Domain.IDomainEvent));

        // Act
        var result = domainEventTypes
            .Should()
            .HaveNameEndingWith("Event")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All domain events should end with 'Event' suffix");
    }

    [Fact]
    public void Configurations_ShouldEndWithConfiguration()
    {
        // Arrange
        var configurationTypes = Types.InAssembly(CatalogInfrastructureAssembly)
            .That()
            .ResideInNamespace("Modules.Catalog.Infrastructure.Persistence.Configurations");

        // Act
        var result = configurationTypes
            .Should()
            .HaveNameEndingWith("Configuration")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All EF Core configurations should end with 'Configuration' suffix");
    }

    [Fact]
    public void ValueObjects_ShouldNotHaveSetPrefix()
    {
        // Arrange
        var valueObjectTypes = Types.InAssembly(CatalogDomainAssembly)
            .That()
            .Inherit(typeof(Common.Domain.ValueObject));

        // Act
        var methods = valueObjectTypes.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.Name.StartsWith("Set", StringComparison.Ordinal));

        // Assert
        methods.Should().BeEmpty(
            "Value objects should be immutable and not have setter methods");
    }

    [Fact]
    public void CommandHandlers_ShouldBeInternal()
    {
        // Arrange
        var commandHandlerTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(Common.Application.ICommandHandler<,>));

        // Act
        var result = commandHandlerTypes
            .Should()
            .NotBePublic()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Command handlers should be internal - only commands should be public");
    }

    [Fact]
    public void QueryHandlers_ShouldBeInternal()
    {
        // Arrange
        var queryHandlerTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.IQueryHandler<,>));

        // Act
        var result = queryHandlerTypes
            .Should()
            .NotBePublic()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Query handlers should be internal - only queries should be public");
    }
}
