using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Catalog.Application.Products.Commands.ActivateProduct;
using Modules.Catalog.Application.Products.Commands.CreateProduct;
using Modules.Catalog.Application.Products.Commands.DiscontinueProduct;
using Modules.Catalog.Application.Products.Commands.UpdateProduct;
using Modules.Catalog.Application.Products.Commands.UpdateProductPrice;
using Modules.Catalog.Application.Products.Queries.GetProduct;
using Modules.Catalog.Application.Products.Queries.SearchProducts;
using Modules.Catalog.Presentation.Contracts.Requests;
using Modules.Catalog.Presentation.Contracts.Responses;

namespace Modules.Catalog.Presentation.Endpoints;

/// <summary>
/// Product API endpoints
/// </summary>
public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/products")
            .WithTags("Products")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetProduct)
            .WithName("GetProduct")
            .Produces<ProductResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", SearchProducts)
            .WithName("SearchProducts")
            .Produces<List<ProductResponse>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateProduct)
            .WithName("UpdateProduct")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}/price", UpdateProductPrice)
            .WithName("UpdateProductPrice")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/activate", ActivateProduct)
            .WithName("ActivateProduct")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/discontinue", DiscontinueProduct)
            .WithName("DiscontinueProduct")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetProduct(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(id);
        var result = await sender.Send(query, cancellationToken);

        if (result is null)
        {
            return Results.NotFound();
        }

        var response = new ProductResponse
        {
            Id = result.Id,
            Name = result.Name,
            Description = result.Description,
            Price = result.Price,
            Currency = result.Currency,
            CategoryId = result.CategoryId,
            CategoryName = result.CategoryName,
            Status = result.Status,
            Images = result.Images.Select(img => new ProductImageResponse
            {
                Id = img.Id,
                Url = img.Url,
                DisplayOrder = img.DisplayOrder
            }).ToList(),
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> SearchProducts(
        ISender sender,
        string? searchTerm = null,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchProductsQuery(
            searchTerm,
            categoryId,
            minPrice,
            maxPrice,
            status,
            pageNumber,
            pageSize);

        var result = await sender.Send(query, cancellationToken);

        var response = result.Items.Select(p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Currency = p.Currency,
            CategoryId = p.CategoryId,
            CategoryName = p.CategoryName,
            Status = p.Status,
            Images = p.Images.Select(img => new ProductImageResponse
            {
                Id = img.Id,
                Url = img.Url,
                DisplayOrder = img.DisplayOrder
            }).ToList(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateProduct(
        CreateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.Currency,
            request.CategoryId);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/v1/products/{result.Value}", result.Value)
            : Results.BadRequest(new { error = result.Error.Message });
    }

    private static async Task<IResult> UpdateProduct(
        Guid id,
        UpdateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure && result.Error.Code == "Product.NotFound")
        {
            return Results.NotFound();
        }

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(new { error = result.Error.Message });
    }

    private static async Task<IResult> UpdateProductPrice(
        Guid id,
        UpdateProductPriceRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductPriceCommand(
            id,
            request.Price,
            request.Currency);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure && result.Error.Code == "Product.NotFound")
        {
            return Results.NotFound();
        }

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(new { error = result.Error.Message });
    }

    private static async Task<IResult> ActivateProduct(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ActivateProductCommand(id);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure && result.Error.Code == "Product.NotFound")
        {
            return Results.NotFound();
        }

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(new { error = result.Error.Message });
    }

    private static async Task<IResult> DiscontinueProduct(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new DiscontinueProductCommand(id);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure && result.Error.Code == "Product.NotFound")
        {
            return Results.NotFound();
        }

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(new { error = result.Error.Message });
    }
}
