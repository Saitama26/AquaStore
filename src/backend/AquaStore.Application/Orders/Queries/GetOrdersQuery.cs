using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Application.Models;
using Common.Domain.Results;
using AquaStore.Domain.Orders;
using AquaStore.Domain.Enums;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Orders.Queries;

/// <summary>
/// Запрос на получение заказов текущего пользователя
/// </summary>
public sealed record GetOrdersQuery(
    int PageNumber = 1,
    int PageSize = 10) : IQuery<PagedResult<OrderListItem>>;

public sealed record OrderListItem(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    decimal TotalAmount,
    string Currency,
    int ItemCount,
    DateTime CreatedAt);

internal sealed class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, PagedResult<OrderListItem>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetOrdersQueryHandler(
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<OrderListItem>>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return UserErrors.InvalidCredentials;
        }

        var orders = await _orderRepository.GetByUserIdAsync(
            _currentUserService.UserId.Value,
            cancellationToken);

        var totalCount = orders.Count;

        var items = orders
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderListItem(
                o.Id,
                o.OrderNumber,
                o.Status,
                o.PaymentStatus,
                o.TotalAmount.Amount,
                o.TotalAmount.Currency,
                o.Items.Count,
                o.CreatedAtUtc))
            .ToList();

        return new PagedResult<OrderListItem>(items, totalCount, request.PageNumber, request.PageSize);
    }
}

