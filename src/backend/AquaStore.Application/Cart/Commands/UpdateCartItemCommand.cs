using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Results;
using AquaStore.Domain.Cart;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Cart.Commands;

/// <summary>
/// Команда обновления количества товара в корзине
/// </summary>
public sealed record UpdateCartItemCommand(
    Guid ProductId,
    int Quantity) : ICommand;

internal sealed class UpdateCartItemCommandHandler : ICommandHandler<UpdateCartItemCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCartItemCommandHandler(
        ICartRepository cartRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateCartItemCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure(UserErrors.InvalidCredentials);
        }

        var cart = await _cartRepository.GetByUserIdAsync(
            _currentUserService.UserId.Value,
            cancellationToken);

        if (cart is null)
        {
            return Result.Failure(CartErrors.NotFound(Guid.Empty));
        }

        try
        {
            cart.UpdateItemQuantity(request.ProductId, request.Quantity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(CartErrors.ItemNotFound(request.ProductId));
        }
    }
}

