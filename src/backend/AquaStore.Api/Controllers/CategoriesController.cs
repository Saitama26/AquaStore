using AquaStore.Application.Categories.Commands;
using AquaStore.Application.Categories.Queries;
using AquaStore.Contracts.Categories.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractResponses = AquaStore.Contracts.Categories.Responses;

namespace AquaStore.Api.Controllers;

/// <summary>
/// Контроллер категорий
/// </summary>
public class CategoriesController : ApiController
{
    public CategoriesController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Получить все категории
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ContractResponses.CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var query = new GetCategoriesQuery();

        var result = await Sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить категорию по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ContractResponses.CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var query = new GetCategoryByIdQuery(id);

        var result = await Sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Создать категорию (только Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var command = new CreateCategoryCommand(
            request.Name,
            request.Description,
            request.ParentCategoryId,
            request.ImageUrl);

        var result = await Sender.Send(command);

        return HandleCreatedResult(result);
    }

    /// <summary>
    /// Массово создать категории (только Admin)
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkCreateCategories([FromBody] BulkCreateCategoriesRequest request)
    {
        var command = new BulkCreateCategoriesCommand(
            request.Categories
                .Select(item => new BulkCreateCategoryItemCommand(
                    item.Name,
                    item.Description,
                    item.ImageUrl))
                .ToList());

        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Обновить категорию (только Admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var command = new UpdateCategoryCommand(
            id,
            request.Name,
            request.Description,
            request.ImageUrl);

        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Удалить категорию (только Admin)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var command = new DeleteCategoryCommand(id);

        var result = await Sender.Send(command);

        return HandleResult(result);
    }
}
