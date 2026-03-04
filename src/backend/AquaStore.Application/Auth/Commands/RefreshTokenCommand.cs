using System.Security.Claims;
using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Common.Infrastructure.Authentication;
using AquaStore.Domain.Users;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Auth.Commands;

/// <summary>
/// Команда обновления токена
/// </summary>
public sealed record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken) : ICommand<RefreshTokenResponse>;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);

internal sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Получаем principal из истекшего токена
        var principal = _jwtProvider.GetPrincipalFromExpiredToken(request.AccessToken);

        if (principal is null)
        {
            return UserErrors.InvalidRefreshToken;
        }

        // Получаем userId из claims
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return UserErrors.InvalidRefreshToken;
        }

        // Находим пользователя
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null || !user.IsRefreshTokenValid(request.RefreshToken))
        {
            return UserErrors.InvalidRefreshToken;
        }

        // Генерируем новые токены
        var roles = new[] { user.Role.ToString() };
        var newAccessToken = _jwtProvider.GenerateAccessToken(user.Id, user.Email, roles);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken();

        // Обновляем refresh token
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.SetRefreshToken(newRefreshToken, refreshTokenExpiry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse(
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(15));
    }
}

