using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Catalog.Application.Categories.Commands.CreateCategory;
using Modules.Catalog.Application.Categories.Queries.GetCategories;
using Modules.Catalog.Application.Categories.Queries.GetCategory;
using Modules.Catalog.Presentation.Contracts.Requests;
using Modules.Catalog.Presentation.Contracts.Responses;

namespace Modules.Catalog.Presentation.Endpoints;

/// <summary>
/// Category API endpoints
/// </summary>
public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories")
            .WithTags("Categories")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetCategory)
            .WithName("GetCategory")
            .Produces<CategoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetCategories)
            .WithName("GetCategories")
            .Produces<List<CategoryResponse>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateCategory)
            .WithName("CreateCategory")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetCategory(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCategoryQuery(id);
        var result = await sender.Send(query, cancellationToken);

        if (result is null)
        {
            return Results.NotFound();
        }

        var response = new CategoryResponse
        {
            Id = result.Id,
            Name = result.Name,
            Slug = result.Slug,
            Description = result.Description,
            ParentId = result.ParentId,
            CreatedAt = result.CreatedAt
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetCategories(
        ISender sender,
        Guid? parentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesQuery(parentId);
        var result = await sender.Send(query, cancellationToken);

        var response = result.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            ParentId = c.ParentId,
            CreatedAt = c.CreatedAt
        }).ToList();

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateCategory(
        CreateCategoryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(
            request.Name,
            request.Slug,
            request.Description,
            request.ParentId);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/v1/categories/{result.Value}", result.Value)
            : Results.BadRequest(new { error = result.Error.Message });
    }
}
