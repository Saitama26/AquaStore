using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Results;
using AquaStore.Domain.Orders;
using AquaStore.Domain.Cart;
using AquaStore.Domain.Products;
using AquaStore.Domain.ValueObjects;
using AquaStore.Domain.Errors;
using AquaStore.Domain.Users;

namespace AquaStore.Application.Orders.Commands;

/// <summary>
/// Команда создания заказа из корзины
/// </summary>
public sealed record CreateOrderCommand(
    string City,
    string Street,
    string Building,
    string? Apartment,
    string PostalCode,
    string? CustomerNote = null,
    IReadOnlyCollection<Guid>? ProductIds = null,
    bool BuyNowSingleUnit = false) : ICommand<CreateOrderResponse>;

public sealed record CreateOrderResponse(
    Guid OrderId,
    string OrderNumber);

internal sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IProductRepository productRepository,
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IEmailSender emailSender,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _emailSender = emailSender;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateOrderResponse>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return UserErrors.InvalidCredentials;
        }

        // Получаем корзину
        var cart = await _cartRepository.GetByUserIdAsync(
            _currentUserService.UserId.Value,
            cancellationToken);

        if (cart is null || cart.IsEmpty)
        {
            return OrderErrors.EmptyCart;
        }

        // Создаём адрес доставки
        var address = Address.Create(
            request.City,
            request.Street,
            request.Building,
            request.Apartment,
            request.PostalCode);

        // Создаём заказ
        var order = Order.Create(
            _currentUserService.UserId.Value,
            address,
            request.CustomerNote);

        var requestedProductIds = request.ProductIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToHashSet();

        var cartItemsForOrder = (requestedProductIds is { Count: > 0 }
                ? cart.Items.Where(i => requestedProductIds.Contains(i.ProductId))
                : cart.Items)
            .ToList();

        if (cartItemsForOrder.Count == 0)
        {
            return OrderErrors.EmptyCart;
        }

        var buyNowSingleUnit = request.BuyNowSingleUnit && cartItemsForOrder.Count == 1;

        // Добавляем выбранные товары из корзины и проверяем остатки
        foreach (var cartItem in cartItemsForOrder)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId, cancellationToken);
            var orderQuantity = buyNowSingleUnit ? 1 : cartItem.Quantity;

            if (product is null)
            {
                return ProductErrors.NotFound(cartItem.ProductId);
            }

            if (product.StockQuantity < orderQuantity)
            {
                return ProductErrors.InsufficientStock(
                    cartItem.ProductId,
                    orderQuantity,
                    product.StockQuantity);
            }

            // Добавляем в заказ
            order.AddItem(
                product.Id,
                product.Name,
                cartItem.UnitPrice,
                orderQuantity);

            // Уменьшаем остатки
            product.RemoveStock(orderQuantity);
        }

        // Устанавливаем стоимость доставки (можно сделать расчёт)
        order.SetShippingCost(0); // Бесплатная доставка

        // Сохраняем заказ
        _orderRepository.Add(order);

        // Удаляем из корзины только заказанные позиции
        foreach (var cartItem in cartItemsForOrder)
        {
            if (buyNowSingleUnit && cartItem.Quantity > 1)
            {
                cart.UpdateItemQuantity(cartItem.ProductId, cartItem.Quantity - 1);
            }
            else
            {
                cart.RemoveItem(cartItem.ProductId);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await SendReceiptEmailAsync(order, cancellationToken);

        return new CreateOrderResponse(order.Id, order.OrderNumber);
    }

    private async Task SendReceiptEmailAsync(Order order, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(order.UserId, cancellationToken);
        if (user is null)
        {
            return;
        }

        var subject = $"Квитанция по заказу {order.OrderNumber}";
        var lines = new List<string>
        {
            $"Спасибо за покупку в AquaStore!",
            $"Номер заказа: {order.OrderNumber}",
            $"Дата: {order.CreatedAtUtc:dd.MM.yyyy HH:mm} (UTC)",
            "",
            "Состав заказа:"
        };

        foreach (var item in order.Items)
        {
            lines.Add($"- {item.ProductName} × {item.Quantity} = {item.TotalPrice.Amount:N2} {item.TotalPrice.Currency}");
        }

        lines.Add("");
        lines.Add($"Подытог: {order.SubTotal.Amount:N2} {order.SubTotal.Currency}");
        lines.Add($"Доставка: {order.ShippingCost.Amount:N2} {order.ShippingCost.Currency}");
        if (order.Discount is not null)
        {
            lines.Add($"Скидка: {order.Discount.Amount:N2} {order.Discount.Currency}");
        }
        lines.Add($"Итого: {order.TotalAmount.Amount:N2} {order.TotalAmount.Currency}");

        var body = string.Join(Environment.NewLine, lines);
        try
        {
            await _emailSender.SendEmailAsync(user.Email.Value, subject, body);
        }
        catch
        {
            // Best-effort: order is already created, avoid failing the flow due to email.
        }
    }
}

