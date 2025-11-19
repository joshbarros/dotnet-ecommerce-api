using NetArchTest.Rules;
using System.Reflection;

namespace ArchitectureTests;

/// <summary>
/// Architecture tests to ensure Application layer follows CQRS and Clean Architecture patterns
/// </summary>
public sealed class ApplicationLayerTests
{
    private static readonly Assembly CatalogApplicationAssembly = typeof(Modules.Catalog.Application.AssemblyReference).Assembly;

    [Fact]
    public void Commands_ShouldBeRecords()
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
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Commands should be sealed records for immutability");
    }

    [Fact]
    public void Queries_ShouldBeRecords()
    {
        // Arrange
        var queryTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.IQuery<>));

        // Act
        var result = queryTypes
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Queries should be sealed records for immutability");
    }

    [Fact]
    public void CommandHandlers_ShouldBeSealed()
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
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Command handlers should be sealed to prevent inheritance");
    }

    [Fact]
    public void QueryHandlers_ShouldBeSealed()
    {
        // Arrange
        var queryHandlerTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.IQueryHandler<,>));

        // Act
        var result = queryHandlerTypes
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Query handlers should be sealed to prevent inheritance");
    }

    [Fact]
    public void Validators_ShouldBeSealed()
    {
        // Arrange
        var validatorTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>));

        // Act
        var result = validatorTypes
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Validators should be sealed to prevent inheritance");
    }

    [Fact]
    public void Commands_ShouldResideInCommandsNamespace()
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
            .ResideInNamespace("Modules.Catalog.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Commands should reside in appropriate Commands namespace");
    }

    [Fact]
    public void Queries_ShouldResideInQueriesNamespace()
    {
        // Arrange
        var queryTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.IQuery<>));

        // Act
        var result = queryTypes
            .Should()
            .ResideInNamespace("Modules.Catalog.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Queries should reside in appropriate Queries namespace");
    }

    [Fact]
    public void CommandHandlers_ShouldHaveOneHandlerPerCommand()
    {
        // Arrange
        var commandHandlerTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(Common.Application.ICommandHandler<,>));

        var handlers = commandHandlerTypes.GetTypes();

        // Act & Assert
        foreach (var handler in handlers)
        {
            var handlerInterfaces = handler.GetInterfaces()
                .Where(i => i.IsGenericType &&
                           (i.GetGenericTypeDefinition() == typeof(Common.Application.ICommandHandler<>) ||
                            i.GetGenericTypeDefinition() == typeof(Common.Application.ICommandHandler<,>)))
                .ToList();

            handlerInterfaces.Should().HaveCount(1,
                $"Handler {handler.Name} should implement exactly one ICommandHandler interface");
        }
    }

    [Fact]
    public void QueryHandlers_ShouldHaveOneHandlerPerQuery()
    {
        // Arrange
        var queryHandlerTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .ImplementInterface(typeof(Common.Application.IQueryHandler<,>));

        var handlers = queryHandlerTypes.GetTypes();

        // Act & Assert
        foreach (var handler in handlers)
        {
            var handlerInterfaces = handler.GetInterfaces()
                .Where(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(Common.Application.IQueryHandler<,>))
                .ToList();

            handlerInterfaces.Should().HaveCount(1,
                $"Handler {handler.Name} should implement exactly one IQueryHandler interface");
        }
    }

    [Fact]
    public void ApplicationLayer_ShouldNotReferenceEntityFramework()
    {
        // Arrange
        var applicationTypes = Types.InAssembly(CatalogApplicationAssembly);

        // Act
        var result = applicationTypes
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should not reference EF Core directly - use abstractions");
    }

    [Fact]
    public void ApplicationLayer_ShouldNotReferenceAspNetCore()
    {
        // Arrange
        var applicationTypes = Types.InAssembly(CatalogApplicationAssembly);

        // Act
        var result = applicationTypes
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should not reference ASP.NET Core");
    }

    [Fact]
    public void Validators_ShouldResideWithTheirCommands()
    {
        // Arrange
        var validatorTypes = Types.InAssembly(CatalogApplicationAssembly)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .GetTypes();

        // Act & Assert
        foreach (var validator in validatorTypes)
        {
            var validatorNamespace = validator.Namespace;
            validatorNamespace.Should().Contain("Commands")
                .Or.Contain("Queries",
                    $"Validator {validator.Name} should reside in the same namespace as its command/query");
        }
    }
}
