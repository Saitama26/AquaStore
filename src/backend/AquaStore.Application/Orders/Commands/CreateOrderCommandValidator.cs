using FluentValidation;

namespace AquaStore.Application.Orders.Commands;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required")
            .MaximumLength(200).WithMessage("Street must not exceed 200 characters");

        RuleFor(x => x.Building)
            .NotEmpty().WithMessage("Building is required")
            .MaximumLength(50).WithMessage("Building must not exceed 50 characters");

        RuleFor(x => x.Apartment)
            .MaximumLength(20).WithMessage("Apartment must not exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Apartment));

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");

        RuleFor(x => x.CustomerNote)
            .MaximumLength(500).WithMessage("Note must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.CustomerNote));
    }
}

