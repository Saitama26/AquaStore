using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Categories;

namespace AquaStore.Application.Categories.Queries;

/// <summary>
/// Запрос на получение всех категорий
/// </summary>
public sealed record GetCategoriesQuery(bool OnlyRoot = false) : IQuery<IReadOnlyList<CategoryResponse>>;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    Guid? ParentCategoryId,
    bool IsActive,
    IReadOnlyList<CategoryResponse> SubCategories);

internal sealed class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, IReadOnlyList<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IReadOnlyList<CategoryResponse>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = request.OnlyRoot
            ? await _categoryRepository.GetRootCategoriesAsync(cancellationToken)
            : await _categoryRepository.GetAllAsync(cancellationToken);

        var response = categories
            .Where(c => c.IsActive)
            .Select(MapToResponse)
            .ToList();

        return response;
    }

    private static CategoryResponse MapToResponse(Category category)
    {
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
                .Select(MapToResponse)
                .ToList());
    }
}

