using Common.Application.Abstractions.Messaging;
using Common.Application.Models;
using Common.Domain.Results;
using AquaStore.Domain.Products;
using AquaStore.Domain.Enums;
using AquaStore.Contracts.Products.Responses;
using Microsoft.EntityFrameworkCore;

namespace AquaStore.Application.Products.Queries;

/// <summary>
/// Запрос на получение списка товаров с фильтрацией
/// </summary>
public sealed record GetProductsQuery(
    string? SearchTerm = null,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    FilterType? FilterType = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? InStock = null,
    bool? IsFeatured = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? SortBy = null,
    bool SortDescending = false) : IQuery<PagedResult<ProductResponse>>;

internal sealed class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, PagedResult<ProductResponse>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PagedResult<ProductResponse>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        // Получаем IQueryable с загруженными связанными сущностями
        var query = _productRepository.GetQueryableWithIncludes()
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            var likeTerm = $"%{term}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Name, likeTerm) ||
                EF.Functions.Like(p.Description, likeTerm) ||
                (p.ShortDescription != null && EF.Functions.Like(p.ShortDescription, likeTerm)) ||
                (p.Sku != null && EF.Functions.Like(p.Sku, likeTerm)) ||
                EF.Functions.Like(p.Slug.Value, likeTerm) ||
                (p.Brand != null && EF.Functions.Like(p.Brand.Name, likeTerm)) ||
                (p.Category != null && EF.Functions.Like(p.Category.Name, likeTerm)));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.BrandId.HasValue)
        {
            query = query.Where(p => p.BrandId == request.BrandId.Value);
        }

        if (request.FilterType.HasValue)
        {
            query = query.Where(p => p.FilterType == request.FilterType.Value);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount <= request.MaxPrice.Value);
        }

        if (request.InStock.HasValue)
        {
            query = query.Where(p => p.IsInStock == request.InStock.Value);
        }

        if (request.IsFeatured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);
        }

        // Подсчёт общего количества
        var totalCount = query.Count();

        // Сортировка
        query = request.SortBy?.ToLowerInvariant() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            "price" => request.SortDescending
                ? query.OrderByDescending(p => p.Price.Amount)
                : query.OrderBy(p => p.Price.Amount),
            "created" => request.SortDescending
                ? query.OrderByDescending(p => p.CreatedAtUtc)
                : query.OrderBy(p => p.CreatedAtUtc),
            _ => query.OrderByDescending(p => p.CreatedAtUtc)
        };

        // Пагинация
        var pagedProducts = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var items = pagedProducts.Select(p =>
        {
            var mainImage = p.Images.FirstOrDefault(i => i.IsMain);
            var imageUrl = mainImage?.Url ?? p.Images.FirstOrDefault()?.Url;

            return new ProductResponse(
                p.Id,
                p.Name,
                p.Slug.Value,
                p.ShortDescription,
                p.Price.Amount,
                p.OldPrice?.Amount,
                imageUrl,
                p.Category.Name,
                p.Brand.Name,
                (int)p.FilterType,
                p.FilterType.ToString(),
                p.StockQuantity,
                p.IsInStock,
                p.IsFeatured,
                p.AverageRating,
                p.Reviews.Count);
        }).ToList();

        return new PagedResult<ProductResponse>(items, totalCount, request.PageNumber, request.PageSize);
    }
}

