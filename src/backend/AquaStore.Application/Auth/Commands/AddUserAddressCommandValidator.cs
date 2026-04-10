using FluentValidation;

namespace AquaStore.Application.Auth.Commands;

public sealed class AddUserAddressCommandValidator : AbstractValidator<AddUserAddressCommand>
{
    public AddUserAddressCommandValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Укажите город")
            .MaximumLength(100).WithMessage("Город не более 100 символов");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Укажите улицу")
            .MaximumLength(150).WithMessage("Улица не более 150 символов");

        RuleFor(x => x.Building)
            .NotEmpty().WithMessage("Укажите дом")
            .MaximumLength(50).WithMessage("Дом не более 50 символов");

        RuleFor(x => x.Apartment)
            .MaximumLength(50).WithMessage("Квартира не более 50 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.Apartment));

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Укажите почтовый индекс")
            .MaximumLength(20).WithMessage("Индекс не более 20 символов");
    }
}
