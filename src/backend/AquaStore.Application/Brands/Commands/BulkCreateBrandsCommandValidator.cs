using FluentValidation;

namespace AquaStore.Application.Brands.Commands;

public sealed class BulkCreateBrandsCommandValidator : AbstractValidator<BulkCreateBrandsCommand>
{
    public BulkCreateBrandsCommandValidator()
    {
        RuleFor(x => x.Brands)
            .NotEmpty().WithMessage("Добавьте хотя бы один бренд для импорта");

        RuleForEach(x => x.Brands)
            .SetValidator(new BulkCreateBrandItemCommandValidator());
    }
}

public sealed class BulkCreateBrandItemCommandValidator : AbstractValidator<BulkCreateBrandItemCommand>
{
    public BulkCreateBrandItemCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название бренда обязательно")
            .MaximumLength(100).WithMessage("Название бренда не более 100 символов");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание бренда не более 1000 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Страна не более 100 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.Country));
    }
}
