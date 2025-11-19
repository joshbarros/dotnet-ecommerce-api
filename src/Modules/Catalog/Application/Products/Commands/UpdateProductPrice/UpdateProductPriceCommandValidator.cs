using FluentValidation;

namespace Modules.Catalog.Application.Products.Commands.UpdateProductPrice;

/// <summary>
/// Validator for UpdateProductPriceCommand
/// </summary>
public sealed class UpdateProductPriceCommandValidator
    : AbstractValidator<UpdateProductPriceCommand>
{
    public UpdateProductPriceCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero")
            .LessThan(1_000_000)
            .WithMessage("Price cannot exceed 1,000,000");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be uppercase letters");
    }
}
