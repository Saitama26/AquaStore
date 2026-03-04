using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AquaStore.Domain.Products;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Products.Commands;

/// <summary>
/// Команда удаления товара
/// </summary>
public sealed record DeleteProductCommand(Guid ProductId) : ICommand;

internal sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(request.ProductId));
        }

        // Деактивируем вместо удаления (soft delete)
        product.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

