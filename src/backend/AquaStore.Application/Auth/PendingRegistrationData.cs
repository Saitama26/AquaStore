namespace AquaStore.Application.Auth;

internal sealed record PendingRegistrationData(
    string Email,
    string PasswordHash,
    string FirstName,
    string LastName,
    string? Phone,
    string Code,
    DateTime ExpiresAtUtc);

