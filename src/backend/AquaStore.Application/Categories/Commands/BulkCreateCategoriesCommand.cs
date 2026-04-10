using AquaStore.Domain.Categories;
using AquaStore.Domain.ValueObjects;
using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;

namespace AquaStore.Application.Categories.Commands;

public sealed record BulkCreateCategoriesCommand(
    IReadOnlyList<BulkCreateCategoryItemCommand> Categories) : ICommand;

public sealed record BulkCreateCategoryItemCommand(
    string Name,
    string? Description,
    string? ImageUrl);

internal sealed class BulkCreateCategoriesCommandHandler : ICommandHandler<BulkCreateCategoriesCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BulkCreateCategoriesCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(BulkCreateCategoriesCommand request, CancellationToken cancellationToken)
    {
        var existingCategories = await _categoryRepository.GetAllAsync(cancellationToken);
        var categorySlugs = existingCategories
            .Select(c => c.Slug.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var hasChanges = false;

        foreach (var item in request.Categories)
        {
            var slug = Slug.Create(item.Name).Value;
            if (!categorySlugs.Add(slug))
            {
                continue;
            }

            var category = Category.Create(item.Name.Trim(), item.Description, null, item.ImageUrl);
            _categoryRepository.Add(category);
            hasChanges = true;
        }

        if (hasChanges)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
