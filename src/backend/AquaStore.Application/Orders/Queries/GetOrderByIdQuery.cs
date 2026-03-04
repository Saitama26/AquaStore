using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Results;
using AquaStore.Domain.Orders;
using AquaStore.Domain.Enums;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Orders.Queries;

/// <summary>
/// Запрос на получение заказа по ID
/// </summary>
public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDetailResponse>;

public sealed record OrderDetailResponse(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    OrderAddressResponse ShippingAddress,
    string? CustomerNote,
    decimal SubTotal,
    decimal ShippingCost,
    decimal? Discount,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<OrderItemResponse> Items,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    DateTime CreatedAt);

public sealed record OrderAddressResponse(
    string City,
    string Street,
    string Building,
    string? Apartment,
    string PostalCode);

public sealed record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

internal sealed class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDetailResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<OrderDetailResponse>> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetWithItemsAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            return OrderErrors.NotFound(request.OrderId);
        }

        // Проверяем, что заказ принадлежит текущему пользователю (или админ)
        if (_currentUserService.UserId != order.UserId && !_currentUserService.IsInRole("Admin"))
        {
            return OrderErrors.NotFound(request.OrderId);
        }

        var address = new OrderAddressResponse(
            order.ShippingAddress.City,
            order.ShippingAddress.Street,
            order.ShippingAddress.Building,
            order.ShippingAddress.Apartment,
            order.ShippingAddress.PostalCode);

        var items = order.Items.Select(i => new OrderItemResponse(
            i.ProductId ?? Guid.Empty, // ProductId может быть null если продукт удален
            i.ProductName,
            i.UnitPrice.Amount,
            i.Quantity,
            i.TotalPrice.Amount)).ToList();

        return new OrderDetailResponse(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.PaymentStatus,
            address,
            order.CustomerNote,
            order.SubTotal.Amount,
            order.ShippingCost.Amount,
            order.Discount?.Amount,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            items,
            order.ShippedAt,
            order.DeliveredAt,
            order.CreatedAtUtc);
    }
}

