using FluentValidation;

namespace AquaStore.Application.Categories.Commands;

public sealed class BulkCreateCategoriesCommandValidator : AbstractValidator<BulkCreateCategoriesCommand>
{
    public BulkCreateCategoriesCommandValidator()
    {
        RuleFor(x => x.Categories)
            .NotEmpty().WithMessage("Добавьте хотя бы одну категорию для импорта");

        RuleForEach(x => x.Categories)
            .SetValidator(new BulkCreateCategoryItemCommandValidator());
    }
}

public sealed class BulkCreateCategoryItemCommandValidator : AbstractValidator<BulkCreateCategoryItemCommand>
{
    public BulkCreateCategoryItemCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название категории обязательно")
            .MaximumLength(100).WithMessage("Название категории не более 100 символов");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание категории не более 1000 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).WithMessage("Некорректный URL изображения")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
    }

    private static bool BeAValidUrl(string? url)
    {
        return string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
