using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Categories;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Categories.Queries;

/// <summary>
/// Запрос на получение категории по ID
/// </summary>
public sealed record GetCategoryByIdQuery(Guid CategoryId) : IQuery<CategoryResponse>;

internal sealed class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, CategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryResponse>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return CategoryErrors.NotFound(request.CategoryId);
        }

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ImageUrl,
            category.ParentCategoryId,
            category.IsActive,
            category.SubCategories
                .Where(sc => sc.IsActive)
                .Select(sc => new CategoryResponse(
                    sc.Id, sc.Name, sc.Slug, sc.Description, sc.ImageUrl,
                    sc.ParentCategoryId, sc.IsActive, []))
                .ToList());
    }
}

