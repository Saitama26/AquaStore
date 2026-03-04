using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Brands;
using AquaStore.Domain.Errors;
using AquaStore.Domain.Products;

namespace AquaStore.Application.Brands.Commands;

/// <summary>
/// Команда удаления бренда
/// </summary>
public sealed record DeleteBrandCommand(Guid BrandId) : ICommand;

internal sealed class DeleteBrandCommandHandler : ICommandHandler<DeleteBrandCommand>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBrandCommandHandler(
        IBrandRepository brandRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _brandRepository = brandRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteBrandCommand request,
        CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
        if (brand is null)
        {
            return Result.Failure(BrandErrors.NotFound(request.BrandId));
        }

        var products = await _productRepository.GetByBrandAsync(request.BrandId, cancellationToken);
        if (products.Any())
        {
            return Result.Failure(BrandErrors.CannotDeleteWithProducts);
        }

        brand.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

