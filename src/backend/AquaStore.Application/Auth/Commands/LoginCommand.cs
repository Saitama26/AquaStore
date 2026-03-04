using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Common.Infrastructure.Authentication;
using AquaStore.Domain.Users;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Auth.Commands;

/// <summary>
/// Команда входа в систему
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<LoginResponse>;

public sealed record LoginResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);

internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Находим пользователя
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return UserErrors.InvalidCredentials;
        }

        // Проверяем пароль
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return UserErrors.InvalidCredentials;
        }

        // Проверяем, активен ли аккаунт
        if (!user.IsActive)
        {
            return UserErrors.AccountDisabled;
        }

        if (!user.EmailConfirmed)
        {
            return UserErrors.EmailNotConfirmed;
        }

        // Генерируем токены
        var roles = new[] { user.Role.ToString() };
        var accessToken = _jwtProvider.GenerateAccessToken(user.Id, user.Email, roles);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        // Сохраняем refresh token
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.SetRefreshToken(refreshToken, refreshTokenExpiry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(15)); // Access token expiry
    }
}

