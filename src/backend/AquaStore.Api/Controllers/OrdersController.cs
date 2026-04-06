using System.Text;
using AquaStore.Api.Services;
using AquaStore.Application.Orders.Commands;
using AquaStore.Application.Orders.Queries;
using AquaStore.Contracts.Common;
using AquaStore.Contracts.Orders.Requests;
using AquaStore.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractResponses = AquaStore.Contracts.Orders.Responses;

namespace AquaStore.Api.Controllers;

/// <summary>
/// Контроллер заказов
/// </summary>
[Authorize]
public class OrdersController : ApiController
{
    public OrdersController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Получить заказы текущего пользователя
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ContractResponses.OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 50 ? 10 : pageSize;
        var query = new GetOrdersQuery(pageNumber, pageSize);

        var result = await Sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить заказ по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContractResponses.OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var query = new GetOrderByIdQuery(id);

        var result = await Sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Создать заказ
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContractResponses.CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(
            request.City,
            request.Street,
            request.Building,
            request.Apartment,
            request.PostalCode,
            request.CustomerNote,
            request.ProductIds,
            request.BuyNowSingleUnit);

        var result = await Sender.Send(command);

        return HandleCreatedResult(result);
    }

    /// <summary>
    /// Получить все заказы (только Admin)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<ContractResponses.OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 100 ? 50 : pageSize;
        var query = new GetAllOrdersQuery(pageNumber, pageSize);
        var result = await Sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Получить PDF-чек заказа
    /// </summary>
    [HttpGet("{id:guid}/receipt/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReceiptPdf(Guid id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await Sender.Send(query);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var pdfBytes = OrderReceiptPdfGenerator.Generate(result.Value);
        var safeOrderNumber = result.Value.OrderNumber.Replace(" ", string.Empty);
        var fileName = $"aquastore-receipt-{safeOrderNumber}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    /// <summary>
    /// Обновить статус заказа (только Admin)
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        if (!Enum.IsDefined(typeof(OrderStatus), request.Status))
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Недопустимый статус заказа.",
                [new ApiError("Order.InvalidStatus", "Недопустимый статус заказа.")]));
        }

        var command = new AdminUpdateOrderStatusCommand(id, (OrderStatus)request.Status);
        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить расширенную аналитику заказов (только Admin)
    /// </summary>
    [HttpGet("analytics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] int topProducts = 10,
        [FromQuery] int topUsers = 10)
    {
        var query = new GetOrderAnalyticsQuery(topProducts, topUsers);
        var result = await Sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Экспорт расширенной аналитики по заказам в CSV (только Admin)
    /// </summary>
    [HttpGet("analytics/export/csv")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportAnalyticsCsv(
        [FromQuery] int topProducts = 10,
        [FromQuery] int topUsers = 10)
    {
        var query = new GetOrderAnalyticsQuery(topProducts, topUsers);
        var result = await Sender.Send(query);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var analytics = result.Value;
        var sb = new StringBuilder();
        sb.AppendLine("sep=;");
        sb.AppendLine("AquaStore Ocean Analytics");
        sb.AppendLine($"GeneratedUtc;{analytics.GeneratedAtUtc:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        sb.AppendLine("Overview");
        sb.AppendLine("Metric;Value");
        sb.AppendLine($"TotalOrders;{analytics.TotalOrders}");
        sb.AppendLine($"CancelledOrders;{analytics.CancelledOrders}");
        sb.AppendLine($"DeliveredOrders;{analytics.DeliveredOrders}");
        sb.AppendLine($"UniqueCustomers;{analytics.UniqueCustomers}");
        sb.AppendLine($"TotalRevenue;{analytics.TotalRevenue.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        sb.AppendLine($"AverageOrderValue;{analytics.AverageOrderValue.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        sb.AppendLine($"Currency;{EscapeCsv(analytics.Currency)}");
        sb.AppendLine();

        sb.AppendLine("OrderStatusBreakdown");
        sb.AppendLine("Status;Count;SharePercent");
        foreach (var item in analytics.StatusBreakdown)
        {
            sb.AppendLine($"{EscapeCsv(ToStatusName(item.Status))};{item.Count};{item.SharePercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }
        sb.AppendLine();

        sb.AppendLine("TopProductsByQuantity");
        sb.AppendLine("Rank;Product;UnitsSold;Revenue;OrdersCount");
        for (var i = 0; i < analytics.TopProductsByQuantity.Count; i++)
        {
            var item = analytics.TopProductsByQuantity[i];
            sb.AppendLine($"{i + 1};{EscapeCsv(item.ProductName)};{item.UnitsSold};{item.Revenue.ToString(System.Globalization.CultureInfo.InvariantCulture)};{item.OrdersCount}");
        }
        sb.AppendLine();

        sb.AppendLine("TopProductsByRevenue");
        sb.AppendLine("Rank;Product;Revenue;UnitsSold;OrdersCount");
        for (var i = 0; i < analytics.TopProductsByRevenue.Count; i++)
        {
            var item = analytics.TopProductsByRevenue[i];
            sb.AppendLine($"{i + 1};{EscapeCsv(item.ProductName)};{item.Revenue.ToString(System.Globalization.CultureInfo.InvariantCulture)};{item.UnitsSold};{item.OrdersCount}");
        }
        sb.AppendLine();

        sb.AppendLine("TopUsersBySpend");
        sb.AppendLine("Rank;UserName;UserEmail;OrdersCount;TotalSpent;AverageOrderValue");
        for (var i = 0; i < analytics.TopUsersBySpend.Count; i++)
        {
            var item = analytics.TopUsersBySpend[i];
            sb.AppendLine($"{i + 1};{EscapeCsv(item.UserName)};{EscapeCsv(item.UserEmail)};{item.OrdersCount};{item.TotalSpent.ToString(System.Globalization.CultureInfo.InvariantCulture)};{item.AverageOrderValue.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }
        sb.AppendLine();

        sb.AppendLine("TopUsersByOrdersCount");
        sb.AppendLine("Rank;UserName;UserEmail;OrdersCount;TotalSpent;AverageOrderValue");
        for (var i = 0; i < analytics.TopUsersByOrdersCount.Count; i++)
        {
            var item = analytics.TopUsersByOrdersCount[i];
            sb.AppendLine($"{i + 1};{EscapeCsv(item.UserName)};{EscapeCsv(item.UserEmail)};{item.OrdersCount};{item.TotalSpent.ToString(System.Globalization.CultureInfo.InvariantCulture)};{item.AverageOrderValue.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }

        var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
        var bytes = Encoding.UTF8.GetPreamble().Concat(contentBytes).ToArray();
        var fileName = $"analytics-stat-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Экспорт статистики заказов в CSV (для админа)
    /// </summary>
    [HttpGet("export/csv")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportOrdersCsv()
    {
        // Получаем все заказы (ограничение по странице задаём большим PageSize)
        const int maxPageSize = 100_000;
        var query = new GetAllOrdersQuery(1, maxPageSize);
        var result = await Sender.Send(query);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var orders = result.Value.Items;

        var sb = new StringBuilder();
        sb.AppendLine("OrderNumber;UserEmail;Status;PaymentStatus;TotalAmount;Currency;ItemCount;CreatedAt");

        foreach (var o in orders)
        {
            sb.AppendLine(
                $"{o.OrderNumber};{o.UserEmail};{o.Status};{o.PaymentStatus};{o.TotalAmount.ToString(System.Globalization.CultureInfo.InvariantCulture)};{o.Currency};{o.ItemCount};{o.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"orders-stat-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Отменить заказ
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var command = new CancelOrderCommand(id);

        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Replace("\r", " ").Replace("\n", " ").Trim();
        if (normalized.Contains(';') || normalized.Contains('"'))
        {
            return $"\"{normalized.Replace("\"", "\"\"")}\"";
        }

        return normalized;
    }

    private static string ToStatusName(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Ожидает",
            OrderStatus.Confirmed => "Подтверждён",
            OrderStatus.Processing => "В обработке",
            OrderStatus.Shipped => "Отправлен",
            OrderStatus.Delivered => "Доставлен",
            OrderStatus.Cancelled => "Отменён",
            _ => "Неизвестно"
        };
    }
}
