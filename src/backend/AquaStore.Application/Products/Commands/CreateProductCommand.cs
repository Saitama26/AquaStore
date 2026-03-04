using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Products;
using AquaStore.Domain.Categories;
using AquaStore.Domain.Brands;
using AquaStore.Domain.Enums;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Products.Commands;

/// <summary>
/// Команда создания товара
/// </summary>
public sealed record CreateProductCommand(
    string Name,
    string Description,
    string? ShortDescription,
    decimal Price,
    decimal? OldPrice,
    FilterType FilterType,
    Guid CategoryId,
    Guid BrandId,
    int StockQuantity = 0,
    string? Sku = null,
    int? FilterLifespanMonths = null,
    int? FilterCapacityLiters = null,
    double? FlowRateLitersPerMinute = null,
    List<string>? ImageUrls = null) : ICommand<Guid>;

internal sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
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

    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        // Проверяем существование категории
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return CategoryErrors.NotFound(request.CategoryId);
        }

        // Проверяем существование бренда
        var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
        if (brand is null)
        {
            return BrandErrors.NotFound(request.BrandId);
        }

        // Создаём товар
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.FilterType,
            request.CategoryId,
            request.BrandId,
            request.StockQuantity,
            request.Sku,
            request.ShortDescription);

        // Устанавливаем характеристики
        product.SetSpecifications(
            request.FilterLifespanMonths,
            request.FilterCapacityLiters,
            request.FlowRateLitersPerMinute);

        // Устанавливаем старую цену (для скидки)
        if (request.OldPrice.HasValue)
        {
            product.SetOldPrice(request.OldPrice.Value);
        }

        // Добавляем изображения
        if (request.ImageUrls is not null)
        {
            for (var i = 0; i < request.ImageUrls.Count; i++)
            {
                product.AddImage(request.ImageUrls[i], isMain: i == 0, sortOrder: i);
            }
        }

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}

