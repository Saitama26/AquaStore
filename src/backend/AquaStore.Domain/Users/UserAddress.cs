using Common.Domain.Primitives;
using AquaStore.Domain.ValueObjects;

namespace AquaStore.Domain.Users;

/// <summary>
/// Адрес пользователя
/// </summary>
public sealed class UserAddress : Entity
{
    public Guid UserId { get; private set; }
    public Address Address { get; private set; } = null!;
    public bool IsDefault { get; private set; }

    private UserAddress() { }

    internal static UserAddress Create(Guid userId, Address address, bool isDefault = false)
    {
        return new UserAddress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Address = address,
            IsDefault = isDefault
        };
    }

    public void Update(Address address)
    {
        Address = address;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
    }

    public void SetAsNonDefault()
    {
        IsDefault = false;
    }
}

