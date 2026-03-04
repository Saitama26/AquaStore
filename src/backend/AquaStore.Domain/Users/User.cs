using Common.Domain.Primitives;
using Common.Domain.Events;
using AquaStore.Domain.Enums;
using AquaStore.Domain.ValueObjects;

namespace AquaStore.Domain.Users;

/// <summary>
/// Пользователь системы
/// </summary>
public sealed class User : AggregateRoot, IAuditableEntity
{
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public PhoneNumber? Phone { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<UserAddress> _addresses = [];
    public IReadOnlyCollection<UserAddress> Addresses => _addresses.AsReadOnly();

    private User() { }

    public static User Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        string? phone = null,
        UserRole role = UserRole.Customer)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = Email.Create(email),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Phone = phone is not null ? PhoneNumber.Create(phone) : null,
            Role = role,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Email));

        return user;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateProfile(string firstName, string lastName, string? phone)
    {
        FirstName = firstName;
        LastName = lastName;
        Phone = phone is not null ? PhoneNumber.Create(phone) : null;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    public void SetRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
    }

    public bool IsRefreshTokenValid(string refreshToken)
    {
        return RefreshToken == refreshToken &&
               RefreshTokenExpiryTime > DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void SetRole(UserRole role) => Role = role;

    public void AddAddress(
        string city,
        string street,
        string building,
        string? apartment,
        string postalCode,
        bool isDefault = false)
    {
        if (isDefault)
        {
            foreach (var addr in _addresses.Where(a => a.IsDefault))
            {
                addr.SetAsNonDefault();
            }
        }

        var address = UserAddress.Create(
            Id,
            Address.Create(city, street, building, apartment, postalCode),
            isDefault);

        _addresses.Add(address);
    }

    public void RemoveAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (address is not null)
        {
            _addresses.Remove(address);
        }
    }

    public void SetDefaultAddress(Guid addressId)
    {
        foreach (var addr in _addresses)
        {
            if (addr.Id == addressId)
                addr.SetAsDefault();
            else
                addr.SetAsNonDefault();
        }
    }

    public UserAddress? DefaultAddress => _addresses.FirstOrDefault(a => a.IsDefault);
}

/// <summary>
/// Событие регистрации пользователя
/// </summary>
public sealed record UserRegisteredEvent(Guid UserId, string Email) : DomainEvent;

