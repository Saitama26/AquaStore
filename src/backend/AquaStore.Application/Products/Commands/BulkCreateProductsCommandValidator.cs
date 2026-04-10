using FluentValidation;

namespace AquaStore.Application.Products.Commands;

public sealed class BulkCreateProductsCommandValidator : AbstractValidator<BulkCreateProductsCommand>
{
    public BulkCreateProductsCommandValidator()
    {
        RuleFor(x => x.Products)
            .NotEmpty().WithMessage("Добавьте хотя бы один товар для импорта");

        RuleForEach(x => x.Products)
            .SetValidator(new BulkCreateProductItemCommandValidator());
    }
}

public sealed class BulkCreateProductItemCommandValidator : AbstractValidator<BulkCreateProductItemCommand>
{
    public BulkCreateProductItemCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название товара обязательно")
            .MaximumLength(200).WithMessage("Название товара не более 200 символов");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание товара обязательно")
            .MaximumLength(5000).WithMessage("Описание товара не более 5000 символов");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500).WithMessage("Краткое описание не более 500 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.ShortDescription));

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Цена должна быть больше нуля");

        RuleFor(x => x.OldPrice)
            .GreaterThan(x => x.Price).WithMessage("Старая цена должна быть больше текущей")
            .When(x => x.OldPrice.HasValue);

        RuleFor(x => x.FilterType)
            .InclusiveBetween(0, 7).WithMessage("Некорректный тип фильтра");

        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Название категории обязательно")
            .MaximumLength(100).WithMessage("Название категории не более 100 символов");

        RuleFor(x => x.BrandName)
            .NotEmpty().WithMessage("Название бренда обязательно")
            .MaximumLength(100).WithMessage("Название бренда не более 100 символов");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Остаток не может быть отрицательным");

        RuleFor(x => x.Sku)
            .MaximumLength(50).WithMessage("SKU не более 50 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.Sku));

        RuleFor(x => x.FilterLifespanMonths)
            .GreaterThan(0).WithMessage("Ресурс фильтра должен быть положительным")
            .When(x => x.FilterLifespanMonths.HasValue);

        RuleFor(x => x.FilterCapacityLiters)
            .GreaterThan(0).WithMessage("Объем фильтра должен быть положительным")
            .When(x => x.FilterCapacityLiters.HasValue);

        RuleFor(x => x.FlowRateLitersPerMinute)
            .GreaterThan(0).WithMessage("Производительность должна быть положительной")
            .When(x => x.FlowRateLitersPerMinute.HasValue);
    }
}
