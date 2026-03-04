using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Categories;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Categories.Commands;

/// <summary>
/// Команда создания категории
/// </summary>
public sealed record CreateCategoryCommand(
    string Name,
    string? Description = null,
    Guid? ParentCategoryId = null,
    string? ImageUrl = null) : ICommand<Guid>;

internal sealed class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        // Проверяем родительскую категорию
        if (request.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parent is null)
            {
                return CategoryErrors.NotFound(request.ParentCategoryId.Value);
            }
        }

        var category = Category.Create(
            request.Name,
            request.Description,
            request.ParentCategoryId,
            request.ImageUrl);

        _categoryRepository.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}

