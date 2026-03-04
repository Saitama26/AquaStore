using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Categories;
using AquaStore.Domain.Products;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Categories.Commands;

/// <summary>
/// Команда удаления категории
/// </summary>
public sealed record DeleteCategoryCommand(Guid CategoryId) : ICommand;

internal sealed class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure(CategoryErrors.NotFound(request.CategoryId));
        }

        // Проверяем наличие подкатегорий
        if (category.SubCategories.Any())
        {
            return Result.Failure(CategoryErrors.CannotDeleteWithSubcategories);
        }

        // Проверяем наличие товаров
        var products = await _productRepository.GetByCategoryAsync(request.CategoryId, cancellationToken);
        if (products.Any())
        {
            return Result.Failure(CategoryErrors.CannotDeleteWithProducts);
        }

        category.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

