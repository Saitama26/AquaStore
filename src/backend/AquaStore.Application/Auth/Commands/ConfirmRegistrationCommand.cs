using Common.Application.Abstractions.Data;
using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Results;
using AquaStore.Domain.Errors;
using AquaStore.Domain.Users;

namespace AquaStore.Application.Auth.Commands;

/// <summary>
/// Команда подтверждения регистрации по коду
/// </summary>
public sealed record ConfirmRegistrationCommand(
    string Email,
    string Code) : ICommand<RegisterResponse>;

public sealed record RegisterResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName);

internal sealed class ConfirmRegistrationCommandHandler
    : ICommandHandler<ConfirmRegistrationCommand, RegisterResponse>
{
    private const string CacheKeyPrefix = "registration:";

    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public ConfirmRegistrationCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Result<RegisterResponse>> Handle(
        ConfirmRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var cacheKey = BuildCacheKey(normalizedEmail);
        var pending = await _cacheService.GetAsync<PendingRegistrationData>(
            cacheKey,
            cancellationToken);

        if (pending is null)
        {
            return UserErrors.RegistrationNotFound;
        }

        if (pending.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);
            return UserErrors.RegistrationCodeExpired;
        }

        if (!string.Equals(pending.Code, request.Code, StringComparison.Ordinal))
        {
            return UserErrors.RegistrationCodeInvalid;
        }

        if (await _userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return UserErrors.EmailAlreadyExists(normalizedEmail);
        }

        var user = User.Create(
            pending.Email,
            pending.PasswordHash,
            pending.FirstName,
            pending.LastName,
            pending.Phone);
        user.ConfirmEmail();

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        return new RegisterResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName);
    }

    private static string BuildCacheKey(string email) => $"{CacheKeyPrefix}{email}";

}

