using Common.Domain.Primitives;

namespace AquaStore.Domain.ValueObjects;

/// <summary>
/// Рейтинг (1-5 звёзд)
/// </summary>
public sealed class Rating : ValueObject
{
    public int Value { get; }

    private Rating(int value)
    {
        Value = value;
    }

    public static Rating Create(int value)
    {
        if (value is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between 1 and 5");

        return new Rating(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value}/5";

    public static implicit operator int(Rating rating) => rating.Value;
}

