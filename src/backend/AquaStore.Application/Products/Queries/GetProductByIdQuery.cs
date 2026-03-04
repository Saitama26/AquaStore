using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Products;
using AquaStore.Domain.Enums;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Products.Queries;

/// <summary>
/// Запрос на получение товара по ID
/// </summary>
public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDetailResponse>;

public sealed record ProductDetailResponse(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string? ShortDescription,
    decimal Price,
    decimal? OldPrice,
    string Currency,
    FilterType FilterType,
    int StockQuantity,
    string? Sku,
    bool IsActive,
    bool IsFeatured,
    int? FilterLifespanMonths,
    int? FilterCapacityLiters,
    double? FlowRateLitersPerMinute,
    Guid CategoryId,
    string CategoryName,
    Guid BrandId,
    string BrandName,
    IReadOnlyList<ProductImageResponse> Images,
    double? AverageRating,
    int ReviewCount,
    DateTime CreatedAt);

public sealed record ProductImageResponse(
    Guid Id,
    string Url,
    string? AltText,
    bool IsMain,
    int SortOrder);

internal sealed class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDetailResponse>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDetailResponse>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return ProductErrors.NotFound(request.ProductId);
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

