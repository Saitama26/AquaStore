using FluentValidation;

namespace AquaStore.Application.Auth.Commands;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Укажите email")
            .EmailAddress().WithMessage("Некорректный формат email")
            .MaximumLength(256).WithMessage("Email не более 256 символов");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Укажите пароль")
            .MinimumLength(8).WithMessage("Пароль не менее 8 символов")
            .MaximumLength(100).WithMessage("Пароль не более 100 символов")
            .Matches("[A-Z]").WithMessage("Пароль должен содержать заглавную букву")
            .Matches("[a-z]").WithMessage("Пароль должен содержать строчную букву")
            .Matches("[0-9]").WithMessage("Пароль должен содержать цифру");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Повторите пароль")
            .Equal(x => x.Password).WithMessage("Пароли не совпадают");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Укажите имя")
            .MaximumLength(100).WithMessage("Имя не более 100 символов");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Укажите фамилию")
            .MaximumLength(100).WithMessage("Фамилия не более 100 символов");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Укажите телефон")
            .Matches(@"^[\d\+\-\(\)\s]*$").WithMessage("Некорректный формат телефона");
    }
}

