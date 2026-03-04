using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Categories;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Categories.Commands;

/// <summary>
/// Команда обновления категории
/// </summary>
public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    string? ImageUrl) : ICommand;

internal sealed class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure(CategoryErrors.NotFound(request.CategoryId));
        }

        category.Update(request.Name, request.Description, request.ImageUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

