using FluentValidation;

namespace AquaStore.Application.Products.Commands;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500).WithMessage("Short description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ShortDescription));

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.OldPrice)
            .GreaterThan(x => x.Price).WithMessage("Old price must be greater than current price")
            .When(x => x.OldPrice.HasValue);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Brand is required");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.Sku)
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Sku));

        RuleFor(x => x.FilterLifespanMonths)
            .GreaterThan(0).WithMessage("Filter lifespan must be positive")
            .When(x => x.FilterLifespanMonths.HasValue);

        RuleFor(x => x.FilterCapacityLiters)
            .GreaterThan(0).WithMessage("Filter capacity must be positive")
            .When(x => x.FilterCapacityLiters.HasValue);

        RuleFor(x => x.FlowRateLitersPerMinute)
            .GreaterThan(0).WithMessage("Flow rate must be positive")
            .When(x => x.FlowRateLitersPerMinute.HasValue);
    }
}

