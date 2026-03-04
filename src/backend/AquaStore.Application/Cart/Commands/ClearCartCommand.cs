using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Results;
using AquaStore.Domain.Cart;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Cart.Commands;

/// <summary>
/// Команда очистки корзины
/// </summary>
public sealed record ClearCartCommand : ICommand;

internal sealed class ClearCartCommandHandler : ICommandHandler<ClearCartCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ClearCartCommandHandler(
        ICartRepository cartRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ClearCartCommand request,
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
            return Result.Success(); // Корзина и так пуста
        }

        cart.Clear();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

