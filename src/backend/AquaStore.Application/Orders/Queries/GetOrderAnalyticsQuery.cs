using AquaStore.Domain.Enums;
using AquaStore.Domain.Orders;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;

namespace AquaStore.Application.Orders.Queries;

/// <summary>
/// Запрос агрегированной аналитики по заказам для админ-панели
/// </summary>
public sealed record GetOrderAnalyticsQuery(
    int TopProducts = 10,
    int TopUsers = 10) : IQuery<OrderAnalyticsResponse>;

public sealed record OrderAnalyticsResponse(
    DateTime GeneratedAtUtc,
    string Currency,
    int TotalOrders,
    int CancelledOrders,
    int DeliveredOrders,
    int UniqueCustomers,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    IReadOnlyList<OrderStatusAnalyticsItem> StatusBreakdown,
    IReadOnlyList<ProductAnalyticsItem> TopProductsByQuantity,
    IReadOnlyList<ProductAnalyticsItem> TopProductsByRevenue,
    IReadOnlyList<UserAnalyticsItem> TopUsersBySpend,
    IReadOnlyList<UserAnalyticsItem> TopUsersByOrdersCount);

public sealed record OrderStatusAnalyticsItem(
    OrderStatus Status,
    int Count,
    decimal SharePercent);

public sealed record ProductAnalyticsItem(
    Guid? ProductId,
    string ProductName,
    int UnitsSold,
    decimal Revenue,
    int OrdersCount);

public sealed record UserAnalyticsItem(
    Guid UserId,
    string UserName,
    string UserEmail,
    int OrdersCount,
    decimal TotalSpent,
    decimal AverageOrderValue);

internal sealed class GetOrderAnalyticsQueryHandler : IQueryHandler<GetOrderAnalyticsQuery, OrderAnalyticsResponse>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderAnalyticsQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderAnalyticsResponse>> Handle(
        GetOrderAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);

        var topProductsLimit = Math.Clamp(request.TopProducts, 3, 25);
        var topUsersLimit = Math.Clamp(request.TopUsers, 3, 25);

        var totalOrders = orders.Count;
        var cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);
        var deliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered);
        var uniqueCustomers = orders.Select(o => o.UserId).Distinct().Count();

        var revenueOrders = orders.Where(o => o.Status != OrderStatus.Cancelled).ToList();
        var totalRevenue = revenueOrders.Sum(o => o.TotalAmount.Amount);
        var averageOrderValue = totalOrders > 0
            ? Math.Round(orders.Average(o => o.TotalAmount.Amount), 2)
            : 0m;

        var currency = orders
            .Select(o => o.TotalAmount.Currency)
            .FirstOrDefault(c => !string.IsNullOrWhiteSpace(c)) ?? "BYN";

        var statusBreakdown = Enum.GetValues<OrderStatus>()
            .Select(status =>
            {
                var count = orders.Count(o => o.Status == status);
                var share = totalOrders > 0
                    ? Math.Round(count * 100m / totalOrders, 2)
                    : 0m;

                return new OrderStatusAnalyticsItem(status, count, share);
            })
            .ToList();

        var productRows = orders
            .SelectMany(order => order.Items.Select(item => new
            {
                order.Id,
                item.ProductId,
                ProductName = string.IsNullOrWhiteSpace(item.ProductName) ? "Товар без названия" : item.ProductName,
                item.Quantity,
                Revenue = item.TotalPrice.Amount
            }))
            .ToList();

        var productStats = productRows
            .GroupBy(x => new { x.ProductId, x.ProductName })
            .Select(group => new ProductAnalyticsItem(
                group.Key.ProductId,
                group.Key.ProductName,
                group.Sum(x => x.Quantity),
                Math.Round(group.Sum(x => x.Revenue), 2),
                group.Select(x => x.Id).Distinct().Count()))
            .ToList();

        var topProductsByQuantity = productStats
            .OrderByDescending(x => x.UnitsSold)
            .ThenByDescending(x => x.Revenue)
            .ThenBy(x => x.ProductName)
            .Take(topProductsLimit)
            .ToList();

        var topProductsByRevenue = productStats
            .OrderByDescending(x => x.Revenue)
            .ThenByDescending(x => x.UnitsSold)
            .ThenBy(x => x.ProductName)
            .Take(topProductsLimit)
            .ToList();

        var userStats = orders
            .GroupBy(order => new
            {
                order.UserId,
                UserName = order.User != null
                    ? $"{order.User.FirstName} {order.User.LastName}".Trim()
                    : "Пользователь",
                UserEmail = order.User?.Email?.Value ?? "—"
            })
            .Select(group =>
            {
                var completedOrders = group.Where(x => x.Status != OrderStatus.Cancelled).ToList();
                var ordersCount = group.Count();
                var totalSpent = Math.Round(completedOrders.Sum(x => x.TotalAmount.Amount), 2);
                var avg = ordersCount > 0 ? Math.Round(totalSpent / ordersCount, 2) : 0m;

                return new UserAnalyticsItem(
                    group.Key.UserId,
                    group.Key.UserName,
                    group.Key.UserEmail,
                    ordersCount,
                    totalSpent,
                    avg);
            })
            .ToList();

        var topUsersBySpend = userStats
            .OrderByDescending(x => x.TotalSpent)
            .ThenByDescending(x => x.OrdersCount)
            .ThenBy(x => x.UserName)
            .Take(topUsersLimit)
            .ToList();

        var topUsersByOrdersCount = userStats
            .OrderByDescending(x => x.OrdersCount)
            .ThenByDescending(x => x.TotalSpent)
            .ThenBy(x => x.UserName)
            .Take(topUsersLimit)
            .ToList();

        return new OrderAnalyticsResponse(
            DateTime.UtcNow,
            currency,
            totalOrders,
            cancelledOrders,
            deliveredOrders,
            uniqueCustomers,
            Math.Round(totalRevenue, 2),
            averageOrderValue,
            statusBreakdown,
            topProductsByQuantity,
            topProductsByRevenue,
            topUsersBySpend,
            topUsersByOrdersCount);
    }
}
