using Common.Domain.Primitives;

namespace AquaStore.Domain.ValueObjects;

/// <summary>
/// Адрес доставки
/// </summary>
public sealed class Address : ValueObject
{
    public string City { get; }
    public string Street { get; }
    public string Building { get; }
    public string? Apartment { get; }
    public string PostalCode { get; }

    private Address(string city, string street, string building, string? apartment, string postalCode)
    {
        City = city;
        Street = street;
        Building = building;
        Apartment = apartment;
        PostalCode = postalCode;
    }

    public static Address Create(
        string city,
        string street,
        string building,
        string? apartment,
        string postalCode)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));

        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required", nameof(street));

        if (string.IsNullOrWhiteSpace(building))
            throw new ArgumentException("Building is required", nameof(building));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code is required", nameof(postalCode));

        return new Address(
            city.Trim(),
            street.Trim(),
            building.Trim(),
            apartment?.Trim(),
            postalCode.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return City;
        yield return Street;
        yield return Building;
        yield return Apartment;
        yield return PostalCode;
    }

    public override string ToString()
    {
        var apt = string.IsNullOrEmpty(Apartment) ? "" : $", кв. {Apartment}";
        return $"{PostalCode}, г. {City}, ул. {Street}, д. {Building}{apt}";
    }
}

