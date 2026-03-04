using AquaStore.Application.Cart.Commands;
using AquaStore.Application.Cart.Queries;
using AquaStore.Contracts.Cart.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractResponses = AquaStore.Contracts.Cart.Responses;

namespace AquaStore.Api.Controllers;

/// <summary>
/// Контроллер корзины
/// </summary>
[Authorize]
public class CartController : ApiController
{
    public CartController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Получить корзину текущего пользователя
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ContractResponses.CartResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCart()
    {
        var query = new GetCartQuery();

        var result = await Sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Добавить товар в корзину
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var command = new AddToCartCommand(request.ProductId, request.Quantity);

        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Обновить количество товара в корзине
    /// </summary>
    [HttpPut("items/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCartItem(Guid productId, [FromBody] UpdateCartItemRequest request)
    {
        var command = new UpdateCartItemCommand(productId, request.Quantity);

        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Удалить товар из корзины
    /// </summary>
    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromCart(Guid productId)
    {
        var command = new RemoveFromCartCommand(productId);

        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Очистить корзину
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCart()
    {
        var command = new ClearCartCommand();

        var result = await Sender.Send(command);

        return HandleResult(result);
    }
}
