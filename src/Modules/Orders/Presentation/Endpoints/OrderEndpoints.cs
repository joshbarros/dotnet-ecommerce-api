using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Orders.Application.Orders;
using Modules.Orders.Application.Orders.AddOrderItem;
using Modules.Orders.Application.Orders.CancelOrder;
using Modules.Orders.Application.Orders.ConfirmOrder;
using Modules.Orders.Application.Orders.CreateOrder;
using Modules.Orders.Application.Orders.GetCustomerOrders;
using Modules.Orders.Application.Orders.GetOrder;
using Modules.Orders.Application.Orders.RemoveOrderItem;
using Modules.Orders.Application.Orders.ShipOrder;
using Modules.Orders.Application.Orders.SubmitOrder;

namespace Modules.Orders.Presentation.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .WithTags("Orders")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetOrder)
            .WithName("GetOrder")
            .Produces<OrderDto>()
            .Produces(404);

        group.MapGet("/customer/{customerId:guid}", GetCustomerOrders)
            .WithName("GetCustomerOrders")
            .Produces<List<OrderDto>>();

        group.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .Produces<Guid>(201)
            .Produces(400);

        group.MapPost("/{id:guid}/items", AddOrderItem)
            .WithName("AddOrderItem")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        group.MapDelete("/{id:guid}/items/{productId:guid}", RemoveOrderItem)
            .WithName("RemoveOrderItem")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        group.MapPost("/{id:guid}/submit", SubmitOrder)
            .WithName("SubmitOrder")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        group.MapPost("/{id:guid}/confirm", ConfirmOrder)
            .WithName("ConfirmOrder")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        group.MapPost("/{id:guid}/ship", ShipOrder)
            .WithName("ShipOrder")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        group.MapPost("/{id:guid}/cancel", CancelOrder)
            .WithName("CancelOrder")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        return app;
    }

    private static async Task<IResult> GetOrder(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(id);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(new { error = result.Error.Message });
    }

    private static async Task<IResult> GetCustomerOrders(
        Guid customerId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerOrdersQuery(customerId);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { error = result.Error.Message });
    }

    private static async Task<IResult> CreateOrder(
        CreateOrderRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.PostalCode);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Results.BadRequest(new { error = result.Error.Message });
        }

        return Results.Created($"/api/v1/orders/{result.Value}", result.Value);
    }

    private static async Task<IResult> AddOrderItem(
        Guid id,
        AddOrderItemRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddOrderItemCommand(
            id,
            request.ProductId,
            request.ProductName,
            request.Quantity,
            request.UnitPrice,
            request.Currency);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Order.NotFound"
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        }

        return Results.NoContent();
    }

    private static async Task<IResult> RemoveOrderItem(
        Guid id,
        Guid productId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RemoveOrderItemCommand(id, productId);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Order.NotFound"
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        }

        return Results.NoContent();
    }

    private static async Task<IResult> SubmitOrder(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new SubmitOrderCommand(id);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Order.NotFound"
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        }

        return Results.NoContent();
    }

    private static async Task<IResult> ConfirmOrder(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmOrderCommand(id);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Order.NotFound"
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        }

        return Results.NoContent();
    }

    private static async Task<IResult> ShipOrder(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ShipOrderCommand(id);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Order.NotFound"
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        }

        return Results.NoContent();
    }

    private static async Task<IResult> CancelOrder(
        Guid id,
        CancelOrderRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand(id, request.Reason);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Order.NotFound"
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        }

        return Results.NoContent();
    }
}

public sealed record CreateOrderRequest(
    Guid CustomerId,
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode);

public sealed record AddOrderItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency);

public sealed record CancelOrderRequest(string Reason);
