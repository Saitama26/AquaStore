using AquaStore.Application.Auth.Commands;
using AquaStore.Application.Auth.Queries;
using AquaStore.Contracts.Auth.Requests;
using AquaStore.Contracts.Users.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractResponses = AquaStore.Contracts.Auth.Responses;
using UserResponses = AquaStore.Contracts.Users.Responses;

namespace AquaStore.Api.Controllers;

/// <summary>
/// Контроллер аутентификации
/// </summary>
public class AuthController : ApiController
{
    public AuthController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ContractResponses.RegisterStartResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.ConfirmPassword,
            request.FirstName,
            request.LastName,
            request.Phone);

        var result = await Sender.Send(command);

        return HandleCreatedResult(result);
    }

    /// <summary>
    /// Подтверждение регистрации
    /// </summary>
    [HttpPost("register/confirm")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ContractResponses.RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmRegistrationRequest request)
    {
        var command = new ConfirmRegistrationCommand(request.Email, request.Code);

        var result = await Sender.Send(command);

        return HandleCreatedResult(result);
    }

    /// <summary>
    /// Вход в систему
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ContractResponses.AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password);

        var result = await Sender.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Получить профиль текущего пользователя
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponses.UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile()
    {
        var query = new GetUserProfileQuery();
        var result = await Sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Обновить профиль текущего пользователя
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var command = new UpdateUserProfileCommand(
            request.FirstName,
            request.LastName,
            request.Phone);

        var result = await Sender.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Обновление токена
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ContractResponses.AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);

        var result = await Sender.Send(command);

        return HandleResult(result);
    }
}
