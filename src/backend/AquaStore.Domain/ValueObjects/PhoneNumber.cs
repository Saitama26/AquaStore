using System.Text.RegularExpressions;
using Common.Domain.Primitives;

namespace AquaStore.Domain.ValueObjects;

/// <summary>
/// Номер телефона
/// </summary>
public sealed partial class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number is required", nameof(phone));

        // Удаляем все кроме цифр и +
        var normalized = PhoneCleanupRegex().Replace(phone, "");

        if (normalized.Length < 10 || normalized.Length > 15)
            throw new ArgumentException("Invalid phone number length", nameof(phone));

        return new PhoneNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;

    [GeneratedRegex(@"[^\d+]", RegexOptions.Compiled)]
    private static partial Regex PhoneCleanupRegex();
}

