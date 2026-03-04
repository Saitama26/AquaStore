using AquaStore.Application.Users.Commands;
using AquaStore.Application.Users.Queries;
using AquaStore.Contracts.Users.Requests;
using AquaStore.Contracts.Users.Responses;
using Common.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaStore.Api.Controllers;

/// <summary>
/// Управление пользователями (для админ-панели)
/// </summary>
[Authorize(Roles = "Admin")]
public sealed class UsersController : ApiController
{
    public UsersController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Получить список пользователей для админ-панели
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminUserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetUsersForAdminQuery(pageNumber, pageSize);
        var result = await Sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Обновить пользователя
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] AdminUpdateUserRequest request)
    {
        var command = new AdminUpdateUserCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Role,
            request.IsActive);

        var result = await Sender.Send(command);
        return HandleResult(result);
    }
}

