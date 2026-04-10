using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Products;
using AquaStore.Domain.Errors;
using AquaStore.Contracts.Products.Responses;

namespace AquaStore.Application.Products.Queries;

/// <summary>
/// Запрос на получение товара по ID
/// </summary>
public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDetailResponse>;

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

        var specifications = new ProductSpecificationsResponse(
            product.FilterLifespanMonths,
            product.FilterCapacityLiters,
            product.FlowRateLitersPerMinute);

        return new ProductDetailResponse(
            product.Id,
            product.Name,
            product.Slug,
            product.Description,
            product.ShortDescription,
            product.Price.Amount,
            product.OldPrice?.Amount,
            product.Price.Currency,
            (int)product.FilterType,
            product.FilterType.ToString(),
            product.StockQuantity,
            product.IsInStock,
            product.Sku,
            product.IsActive,
            product.IsFeatured,
            specifications,
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

