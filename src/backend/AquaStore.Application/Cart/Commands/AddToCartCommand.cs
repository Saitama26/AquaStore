using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Results;
using AquaStore.Domain.Cart;
using AquaStore.Domain.Products;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Cart.Commands;

/// <summary>
/// Команда добавления товара в корзину
/// </summary>
public sealed record AddToCartCommand(
    Guid ProductId,
    int Quantity = 1) : ICommand;

internal sealed class AddToCartCommandHandler : ICommandHandler<AddToCartCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AddToCartCommandHandler(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        AddToCartCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure(UserErrors.InvalidCredentials);
        }

        // Проверяем товар
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(request.ProductId));
        }

        if (!product.IsActive)
        {
            return Result.Failure(ProductErrors.NotFound(request.ProductId));
        }

        // Получаем или создаём корзину
        var cart = await _cartRepository.GetOrCreateAsync(
            _currentUserService.UserId.Value,
            cancellationToken);

        // Добавляем товар
        cart.AddItem(product.Id, product.Price, request.Quantity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

