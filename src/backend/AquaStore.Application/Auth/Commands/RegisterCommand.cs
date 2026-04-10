using System.Security.Cryptography;
using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Results;
using Common.Infrastructure.Authentication;
using AquaStore.Domain.Users;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Auth.Commands;

/// <summary>
/// Команда регистрации пользователя
/// </summary>
public sealed record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string Phone) : ICommand<RegisterStartResponse>;

public sealed record RegisterStartResponse(
    string Email,
    int ExpiresInMinutes);

internal sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterStartResponse>
{
    private const int CodeLength = 6;
    private const int CodeExpirationMinutes = 10;
    private const string CacheKeyPrefix = "registration:";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICacheService _cacheService;
    private readonly IEmailSender _emailSender;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICacheService cacheService,
        IEmailSender emailSender)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _cacheService = cacheService;
        _emailSender = emailSender;
    }

    public async Task<Result<RegisterStartResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Проверяем, существует ли пользователь с таким email
        if (await _userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return UserErrors.EmailAlreadyExists(normalizedEmail);
        }
        var code = GenerateCode();
        var expiresAt = DateTime.UtcNow.AddMinutes(CodeExpirationMinutes);

        var passwordHash = _passwordHasher.Hash(request.Password);
        var pendingRegistration = new PendingRegistrationData(
            normalizedEmail,
            passwordHash,
            request.FirstName,
            request.LastName,
            request.Phone,
            code,
            expiresAt);

        await _cacheService.SetAsync(
            BuildCacheKey(normalizedEmail),
            pendingRegistration,
            TimeSpan.FromMinutes(CodeExpirationMinutes),
            cancellationToken);

        try
        {
            await _emailSender.SendEmailAsync(
                normalizedEmail,
                "Код подтверждения регистрации",
                BuildEmailBody(code),
                cancellationToken);
        }
        catch (Exception)
        {
            return UserErrors.EmailDeliveryFailed;
        }

        return new RegisterStartResponse(normalizedEmail, CodeExpirationMinutes);
    }

    private static string BuildCacheKey(string email) => $"{CacheKeyPrefix}{email}";

    private static string GenerateCode()
    {
        var number = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return number.ToString($"D{CodeLength}");
    }

    private static string BuildEmailBody(string code)
    {
        return $"""
               <h2>Подтверждение регистрации</h2>
               <p>Ваш код подтверждения: <strong>{code}</strong></p>
               <p>Код действует 10 минут.</p>
               """;
    }
}

