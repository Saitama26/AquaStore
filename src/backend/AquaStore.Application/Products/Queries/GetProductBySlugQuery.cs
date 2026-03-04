using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Products;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Products.Queries;

/// <summary>
/// Запрос на получение товара по Slug
/// </summary>
public sealed record GetProductBySlugQuery(string Slug) : IQuery<ProductDetailResponse>;

internal sealed class GetProductBySlugQueryHandler : IQueryHandler<GetProductBySlugQuery, ProductDetailResponse>
{
    private readonly IProductRepository _productRepository;

    public GetProductBySlugQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDetailResponse>> Handle(
        GetProductBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetBySlugAsync(request.Slug, cancellationToken);

        if (product is null)
        {
            return ProductErrors.NotFoundBySlug(request.Slug);
        }

        var images = product.Images
            .OrderByDescending(i => i.IsMain)
            .ThenBy(i => i.SortOrder)
            .Select(i => new ProductImageResponse(i.Id, i.Url, i.AltText, i.IsMain, i.SortOrder))
            .ToList();

        return new ProductDetailResponse(
            product.Id,
            product.Name,
            product.Slug,
            product.Description,
            product.ShortDescription,
            product.Price.Amount,
            product.OldPrice?.Amount,
            product.Price.Currency,
            product.FilterType,
            product.StockQuantity,
            product.Sku,
            product.IsActive,
            product.IsFeatured,
            product.FilterLifespanMonths,
            product.FilterCapacityLiters,
            product.FlowRateLitersPerMinute,
            product.CategoryId,
            product.Category.Name,
            product.BrandId,
            product.Brand.Name,
            images,
            product.AverageRating,
            product.Reviews.Count,
            product.CreatedAtUtc);
    }
}

