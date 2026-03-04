namespace frontend.Models;

public class User
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? Phone { get; private set; }
    public UserRole Role { get; private set; } = UserRole.Customer;
    public bool IsActive { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<UserAddress> _addresses = [];
    public IReadOnlyCollection<UserAddress> Addresses => _addresses.AsReadOnly();
}

public class UserAddress
{
    public string? City { get; }
    public string? Street { get; }
    public string? Building { get; }
    public string? Apartment { get; }
    public string? PostalCode { get; }
}

public enum UserRole : byte
{
    Customer = 0,
    Admin = 1
}