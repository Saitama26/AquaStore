using System.Text;
using AquaStore.Application.Orders.Commands;
using AquaStore.Application.Orders.Queries;
using AquaStore.Contracts.Orders.Requests;
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
}
