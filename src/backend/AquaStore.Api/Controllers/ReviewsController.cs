using AquaStore.Application.Reviews.Commands;
using AquaStore.Application.Reviews.Queries;
using AquaStore.Contracts.Reviews.Requests;
using AquaStore.Contracts.Reviews.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaStore.Api.Controllers;

/// <summary>
/// Контроллер отзывов
/// </summary>
public class ReviewsController : ApiController
{
    public ReviewsController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Получить отзывы по товару
    /// </summary>
    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AquaStore.Contracts.Common.PagedResponse<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProduct(
        Guid productId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > 50 ? 5 : pageSize;

        var query = new GetReviewsByProductIdQuery(productId, pageNumber, pageSize);
        var result = await Sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Добавить отзыв к товару
    /// </summary>
    [HttpPost("product/{productId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(Guid productId, [FromBody] CreateReviewRequest request)
    {
        var command = new CreateReviewCommand(productId, request.Rating, request.Comment);
        var result = await Sender.Send(command);
        return HandleCreatedResult(result);
    }
}

