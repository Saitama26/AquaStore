using AquaStore.Application.Products.Commands;
using AquaStore.Application.Products.Queries;
using AquaStore.Contracts.Products.Requests;
using AquaStore.Contracts.Common;
using AquaStore.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractResponses = AquaStore.Contracts.Products.Responses;

namespace AquaStore.Api.Controllers;

/// <summary>
/// Контроллер товаров
/// </summary>
public class ProductsController : ApiController
{
    public ProductsController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Получить список товаров с фильтрацией
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResponse<ContractResponses.ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterRequest request)
    {
        var query = new GetProductsQuery(
            SearchTerm: request.SearchTerm,
            CategoryId: request.CategoryId,
            BrandId: request.BrandId,
            FilterType: request.FilterType.HasValue ? (FilterType)request.FilterType.Value : null,
            MinPrice: request.MinPrice,
            MaxPrice: request.MaxPrice,
            InStock: request.InStock,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            SortBy: request.SortBy,
            SortDescending: request.SortDescending);

        var result = await Sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить товар по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ContractResponses.ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var query = new GetProductByIdQuery(id);

        var result = await Sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить товар по slug
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ContractResponses.ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductBySlug(string slug)
    {
        var query = new GetProductBySlugQuery(slug);

        var result = await Sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Создать товар (только Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.ShortDescription,
            request.Price,
            request.OldPrice,
            (FilterType)request.FilterType,
            request.CategoryId,
            request.BrandId,
            request.StockQuantity,
            request.Sku,
            request.FilterLifespanMonths,
            request.FilterCapacityLiters,
            request.FlowRateLitersPerMinute,
            request.ImageUrls);

        var result = await Sender.Send(command);

        return HandleCreatedResult(result);
    }

    /// <summary>
    /// Обновить товар (только Admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.ShortDescription,
            request.Price,
            request.OldPrice,
            (FilterType)request.FilterType,
            request.CategoryId,
            request.BrandId,
            request.Sku,
            request.FilterLifespanMonths,
            request.FilterCapacityLiters,
            request.FlowRateLitersPerMinute);

        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Удалить товар (только Admin)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var command = new DeleteProductCommand(id);

        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Обновить остатки товара (только Admin)
    /// </summary>
    [HttpPatch("{id:guid}/stock")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
    {
        var command = new UpdateStockCommand(id, request.Quantity);

        var result = await Sender.Send(command);

        return HandleResult(result);
    }
}
