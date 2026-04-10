using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Services;
using Common.Domain.Results;
using AquaStore.Domain.Users;
using AquaStore.Domain.Errors;

namespace AquaStore.Application.Auth.Queries;

/// <summary>
/// Запрос на получение профиля текущего пользователя
/// </summary>
public sealed record GetUserProfileQuery : IQuery<UserProfileResponse>;

public sealed record UserProfileResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    bool EmailConfirmed,
    IReadOnlyList<UserAddressResponse> Addresses,
    DateTime CreatedAt);

public sealed record UserAddressResponse(
    Guid Id,
    string City,
    string Street,
    string Building,
    string? Apartment,
    string PostalCode,
    bool IsDefault);

internal sealed class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, UserProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserProfileQueryHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserProfileResponse>> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return UserErrors.InvalidCredentials;
        }

        var user = await _userRepository.GetByIdWithAddressesAsync(_currentUserService.UserId.Value, cancellationToken);

        if (user is null)
        {
            return UserErrors.NotFound(_currentUserService.UserId.Value);
        }

        var addresses = user.Addresses
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Id)
            .Select(a => new UserAddressResponse(
                a.Id,
                a.Address.City,
                a.Address.Street,
                a.Address.Building,
                a.Address.Apartment,
                a.Address.PostalCode,
                a.IsDefault))
            .ToList();

        return new UserProfileResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Phone?.ToString(),
            user.Role.ToString(),
            user.EmailConfirmed,
            addresses,
            user.CreatedAtUtc);
    }
}

