using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Customers.Application.Customers;
using Modules.Customers.Application.Customers.AddAddress;
using Modules.Customers.Application.Customers.CreateCustomer;
using Modules.Customers.Application.Customers.GetAllCustomers;
using Modules.Customers.Application.Customers.GetCustomer;
using Modules.Customers.Application.Customers.SuspendCustomer;
using Modules.Customers.Application.Customers.UpdateCustomer;

namespace Modules.Customers.Presentation.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/customers")
            .WithTags("Customers")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetCustomer)
            .WithName("GetCustomer")
            .Produces<CustomerDto>()
            .Produces(404);

        group.MapGet("/", GetAllCustomers)
            .WithName("GetAllCustomers")
            .Produces<List<CustomerDto>>();

        group.MapPost("/", CreateCustomer)
            .WithName("CreateCustomer")
            .Produces<Guid>(201)
            .Produces(400);

        group.MapPut("/{id:guid}", UpdateCustomer)
            .WithName("UpdateCustomer")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        group.MapPost("/{id:guid}/addresses", AddAddress)
            .WithName("AddAddress")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        group.MapPost("/{id:guid}/suspend", SuspendCustomer)
            .WithName("SuspendCustomer")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        return app;
    }

    private static async Task<IResult> GetCustomer(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerQuery(id);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(new { error = result.Error.Message });
    }

    private static async Task<IResult> GetAllCustomers(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetAllCustomersQuery();

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { error = result.Error.Message });
    }

    private static async Task<IResult> CreateCustomer(
        CreateCustomerRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error.Message });
        }

        return Results.Created($"/api/v1/customers/{result.Value}", result.Value);
    }

    private static async Task<IResult> UpdateCustomer(
        Guid id,
        UpdateCustomerRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Customer.NotFound"
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        }

        return Results.NoContent();
    }

    private static async Task<IResult> AddAddress(
        Guid id,
        AddAddressRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddAddressCommand(
            id,
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.PostalCode);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Customer.NotFound"
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        }

        return Results.NoContent();
    }

    private static async Task<IResult> SuspendCustomer(
        Guid id,
        SuspendCustomerRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new SuspendCustomerCommand(id, request.Reason);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Customer.NotFound"
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        }

        return Results.NoContent();
    }
}

public sealed record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);

public sealed record UpdateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);

public sealed record AddAddressRequest(
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode);

public sealed record SuspendCustomerRequest(string Reason);
