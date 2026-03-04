using Common.Application.Abstractions.Messaging;
using Common.Application.Models;
using Common.Domain.Results;
using AquaStore.Domain.Orders;
using AquaStore.Domain.Enums;

namespace AquaStore.Application.Orders.Queries;

/// <summary>
/// Запрос на получение всех заказов (для админа)
/// </summary>
public sealed record GetAllOrdersQuery(
    int PageNumber = 1,
    int PageSize = 50) : IQuery<PagedResult<AdminOrderListItem>>;

public sealed record AdminOrderListItem(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    string UserName,
    string UserEmail,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    decimal TotalAmount,
    string Currency,
    int ItemCount,
    DateTime CreatedAt);

internal sealed class GetAllOrdersQueryHandler : IQueryHandler<GetAllOrdersQuery, PagedResult<AdminOrderListItem>>
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<PagedResult<AdminOrderListItem>>> Handle(
        GetAllOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);

        var totalCount = orders.Count;

        var items = orders
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new AdminOrderListItem(
                o.Id,
                o.OrderNumber,
                o.UserId,
                o.User != null ? $"{o.User.FirstName} {o.User.LastName}".Trim() : "—",
                o.User?.Email?.Value ?? "—",
                o.Status,
                o.PaymentStatus,
                o.TotalAmount.Amount,
                o.TotalAmount.Currency,
                o.Items.Count,
                o.CreatedAtUtc))
            .ToList();

        return new PagedResult<AdminOrderListItem>(items, totalCount, request.PageNumber, request.PageSize);
    }
}

