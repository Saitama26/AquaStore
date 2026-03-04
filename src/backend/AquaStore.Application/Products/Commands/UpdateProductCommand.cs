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
/// Команда обновления товара
/// </summary>
public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string Description,
    string? ShortDescription,
    decimal Price,
    decimal? OldPrice,
    FilterType FilterType,
    Guid CategoryId,
    Guid BrandId,
    string? Sku,
    int? FilterLifespanMonths,
    int? FilterCapacityLiters,
    double? FlowRateLitersPerMinute) : ICommand;

internal sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
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

    public async Task<Result> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(request.ProductId));
        }

        // Проверяем существование категории
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure(CategoryErrors.NotFound(request.CategoryId));
        }

        // Проверяем существование бренда
        var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
        if (brand is null)
        {
            return Result.Failure(BrandErrors.NotFound(request.BrandId));
        }

        // Обновляем товар
        product.Update(
            request.Name,
            request.Description,
            request.ShortDescription,
            request.Price,
            request.FilterType,
            request.CategoryId,
            request.BrandId,
            request.Sku);

        product.SetSpecifications(
            request.FilterLifespanMonths,
            request.FilterCapacityLiters,
            request.FlowRateLitersPerMinute);

        product.SetOldPrice(request.OldPrice);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

