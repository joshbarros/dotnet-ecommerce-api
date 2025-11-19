using FluentValidation;

namespace Modules.Catalog.Application.Products.Queries.SearchProducts;

/// <summary>
/// Validator for SearchProductsQuery
/// </summary>
public sealed class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than zero");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than zero")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum price cannot be negative")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThan(0)
            .WithMessage("Maximum price must be greater than zero")
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Must(x => x.MinPrice <= x.MaxPrice)
            .WithMessage("Minimum price cannot be greater than maximum price")
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);

        RuleFor(x => x.Status)
            .Must(status => new[] { "Draft", "Active", "OutOfStock", "Discontinued" }.Contains(status))
            .WithMessage("Invalid status value")
            .When(x => !string.IsNullOrEmpty(x.Status));
    }
}
