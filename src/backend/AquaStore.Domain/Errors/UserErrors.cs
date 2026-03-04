using Common.Domain.Errors;

namespace AquaStore.Domain.Errors;

/// <summary>
/// Ошибки, связанные с пользователями
/// </summary>
public static class UserErrors
{
    public static Error NotFound(Guid userId) =>
        Error.NotFound("User.NotFound", $"User with ID '{userId}' was not found");

    public static Error NotFoundByEmail(string email) =>
        Error.NotFound("User.NotFoundByEmail", $"User with email '{email}' was not found");

    public static Error EmailAlreadyExists(string email) =>
        Error.Conflict("User.EmailExists", $"User with email '{email}' already exists");

    public static Error InvalidCredentials =>
        Error.Unauthorized("User.InvalidCredentials", "Invalid email or password");

    public static Error AccountDisabled =>
        Error.Forbidden("User.AccountDisabled", "User account is disabled");

    public static Error EmailNotConfirmed =>
        Error.Forbidden("User.EmailNotConfirmed", "Email address has not been confirmed");

    public static Error RegistrationNotFound =>
        Error.NotFound("User.RegistrationNotFound", "Registration request was not found or expired");

    public static Error RegistrationCodeInvalid =>
        Error.Validation("User.RegistrationCodeInvalid", "Invalid registration code");

    public static Error RegistrationCodeExpired =>
        Error.Validation("User.RegistrationCodeExpired", "Registration code has expired");

    public static Error EmailDeliveryFailed =>
        Error.Failure("User.EmailDeliveryFailed", "Unable to send confirmation email");

    public static Error InvalidRefreshToken =>
        Error.Unauthorized("User.InvalidRefreshToken", "Invalid or expired refresh token");

    public static Error PasswordTooWeak =>
        Error.Validation("User.PasswordTooWeak", "Password does not meet security requirements");
}

