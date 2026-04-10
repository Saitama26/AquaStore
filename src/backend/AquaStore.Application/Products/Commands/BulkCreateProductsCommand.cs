using AquaStore.Domain.Brands;
using AquaStore.Domain.Categories;
using AquaStore.Domain.Enums;
using AquaStore.Domain.Products;
using AquaStore.Domain.ValueObjects;
using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;

namespace AquaStore.Application.Products.Commands;

public sealed record BulkCreateProductsCommand(
    IReadOnlyList<BulkCreateProductItemCommand> Products) : ICommand;

public sealed record BulkCreateProductItemCommand(
    string Name,
    string Description,
    string? ShortDescription,
    decimal Price,
    decimal? OldPrice,
    int FilterType,
    string CategoryName,
    string BrandName,
    int StockQuantity,
    string? Sku,
    int? FilterLifespanMonths,
    int? FilterCapacityLiters,
    double? FlowRateLitersPerMinute,
    IReadOnlyList<string>? ImageUrls);

internal sealed class BulkCreateProductsCommandHandler : ICommandHandler<BulkCreateProductsCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BulkCreateProductsCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(BulkCreateProductsCommand request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        var brands = await _brandRepository.GetAllAsync(cancellationToken);
        var products = await _productRepository.GetAllAsync(cancellationToken);

        var categoriesBySlug = categories
            .ToDictionary(c => c.Slug.Value, c => c, StringComparer.OrdinalIgnoreCase);
        var brandsBySlug = brands
            .ToDictionary(b => b.Slug.Value, b => b, StringComparer.OrdinalIgnoreCase);
        var productSlugs = products
            .Select(p => p.Slug.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var hasChanges = false;

        foreach (var item in request.Products)
        {
            var productSlug = Slug.Create(item.Name).Value;
            if (!productSlugs.Add(productSlug))
            {
                continue;
            }

            var category = GetOrCreateCategory(item.CategoryName, categoriesBySlug);
            var brand = GetOrCreateBrand(item.BrandName, brandsBySlug);

            var product = Product.Create(
                item.Name.Trim(),
                item.Description,
                item.Price,
                (FilterType)item.FilterType,
                category.Id,
                brand.Id,
                item.StockQuantity,
                item.Sku,
                item.ShortDescription);

            product.SetSpecifications(
                item.FilterLifespanMonths,
                item.FilterCapacityLiters,
                item.FlowRateLitersPerMinute);

            if (item.OldPrice.HasValue)
            {
                product.SetOldPrice(item.OldPrice.Value);
            }

            if (item.ImageUrls is { Count: > 0 })
            {
                for (var i = 0; i < item.ImageUrls.Count; i++)
                {
                    product.AddImage(item.ImageUrls[i], isMain: i == 0, sortOrder: i);
                }
            }

            _productRepository.Add(product);
            hasChanges = true;
        }

        if (hasChanges)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();

        Category GetOrCreateCategory(string categoryName, IDictionary<string, Category> cache)
        {
            var normalizedName = categoryName.Trim();
            var slug = Slug.Create(normalizedName).Value;

            if (cache.TryGetValue(slug, out var existingCategory))
            {
                return existingCategory;
            }

            var createdCategory = Category.Create(normalizedName);
            _categoryRepository.Add(createdCategory);
            cache[slug] = createdCategory;
            hasChanges = true;
            return createdCategory;
        }

        Brand GetOrCreateBrand(string brandName, IDictionary<string, Brand> cache)
        {
            var normalizedName = brandName.Trim();
            var slug = Slug.Create(normalizedName).Value;

            if (cache.TryGetValue(slug, out var existingBrand))
            {
                return existingBrand;
            }

            var createdBrand = Brand.Create(normalizedName);
            _brandRepository.Add(createdBrand);
            cache[slug] = createdBrand;
            hasChanges = true;
            return createdBrand;
        }
    }
}
