using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Results;
using AquaStore.Domain.Cart;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Cart.Queries;

/// <summary>
/// Запрос на получение корзины текущего пользователя
/// </summary>
public sealed record GetCartQuery : IQuery<CartResponse>;

public sealed record CartResponse(
    Guid Id,
    Guid UserId,
    IReadOnlyList<CartItemResponse> Items,
    decimal TotalAmount,
    string Currency,
    int TotalItems);

public sealed record CartItemResponse(
    Guid ProductId,
    string ProductName,
    string ProductSlug,
    string? ProductImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

internal sealed class GetCartQueryHandler : IQueryHandler<GetCartQuery, CartResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCartQueryHandler(
        ICartRepository cartRepository,
        ICurrentUserService currentUserService)
    {
        _cartRepository = cartRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CartResponse>> Handle(
        GetCartQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return UserErrors.InvalidCredentials;
        }

        var cart = await _cartRepository.GetOrCreateAsync(
            _currentUserService.UserId.Value,
            cancellationToken);

        var items = cart.Items.Select(item => new CartItemResponse(
            item.ProductId,
            item.Product.Name,
            item.Product.Slug,
            item.Product.Images.FirstOrDefault(i => i.IsMain)?.Url,
            item.UnitPrice.Amount,
            item.Quantity,
            item.TotalPrice.Amount)).ToList();

        return new CartResponse(
            cart.Id,
            cart.UserId,
            items,
            cart.TotalAmount.Amount,
            cart.TotalAmount.Currency,
            cart.TotalItems);
    }
}

