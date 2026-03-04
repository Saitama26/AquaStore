using AquaStore.Contracts.Common;
using Common.Domain.Errors;
using Common.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AquaStore.Api.Controllers;

/// <summary>
/// Базовый контроллер API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    protected readonly ISender Sender;

    protected ApiController(ISender sender)
    {
        Sender = sender;
    }

    /// <summary>
    /// Преобразовать Result в IActionResult
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse.Ok());

        return HandleFailure(result);
    }

    /// <summary>
    /// Преобразовать Result<T> в IActionResult
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<T>.Ok(result.Value));

        return HandleFailure(result);
    }

    /// <summary>
    /// Преобразовать Result<T> в Created IActionResult
    /// </summary>
    protected IActionResult HandleCreatedResult<T>(Result<T> result, string? location = null)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<T>.Ok(result.Value);
            if (!string.IsNullOrEmpty(location))
                return Created(location, response);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        return HandleFailure(result);
    }

    /// <summary>
    /// Обработка неуспешного результата (в т.ч. валидация с несколькими ошибками)
    /// </summary>
    private IActionResult HandleFailure(Result result)
    {
        if (result is IValidationResult validationResult && validationResult.Errors.Length > 0)
        {
            var apiErrors = validationResult.Errors
                .Select(e => new ApiError(e.Code, e.Message))
                .ToList();
            var message = string.Join("; ", validationResult.Errors.Select(e => e.Message));
            var response = ApiResponse<object>.Fail(message, apiErrors);
            return BadRequest(response);
        }

        return HandleError(result.Error);
    }

    /// <summary>
    /// Обработка одной ошибки
    /// </summary>
    private IActionResult HandleError(Error error)
    {
        var errors = new List<ApiError> { new(error.Code, error.Message) };
        var response = ApiResponse<object>.Fail(error.Message, errors);

        return error.Type switch
        {
            ErrorType.Validation => BadRequest(response),
            ErrorType.NotFound => NotFound(response),
            ErrorType.Conflict => Conflict(response),
            ErrorType.Unauthorized => Unauthorized(response),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }
}
