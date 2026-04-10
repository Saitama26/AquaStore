using AquaStore.Application.Brands.Commands;
using AquaStore.Application.Brands.Queries;
using AquaStore.Contracts.Brands.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractResponses = AquaStore.Contracts.Brands.Responses;

namespace AquaStore.Api.Controllers;

/// <summary>
/// Контроллер брендов
/// </summary>
public class BrandsController : ApiController
{
    public BrandsController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Получить все бренды
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ContractResponses.BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBrands()
    {
        var query = new GetBrandsQuery();

        var result = await Sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Создать бренд (только Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest request)
    {
        var command = new CreateBrandCommand(
            request.Name,
            request.Description,
            request.Country,
            request.LogoUrl,
            request.WebsiteUrl);

        var result = await Sender.Send(command);

        return HandleCreatedResult(result);
    }

    /// <summary>
    /// Массово создать бренды (только Admin)
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkCreateBrands([FromBody] BulkCreateBrandsRequest request)
    {
        var command = new BulkCreateBrandsCommand(
            request.Brands
                .Select(item => new BulkCreateBrandItemCommand(
                    item.Name,
                    item.Description,
                    item.Country,
                    item.LogoUrl,
                    item.WebsiteUrl))
                .ToList());

        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Обновить бренд (только Admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] UpdateBrandRequest request)
    {
        var command = new UpdateBrandCommand(
            id,
            request.Name,
            request.Description,
            request.Country,
            request.LogoUrl,
            request.WebsiteUrl);

        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Удалить бренд (только Admin)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBrand(Guid id)
    {
        var command = new DeleteBrandCommand(id);
        var result = await Sender.Send(command);
        return HandleResult(result);
    }
}
