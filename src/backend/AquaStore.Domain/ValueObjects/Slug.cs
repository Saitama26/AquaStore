using System.Text;
using System.Text.RegularExpressions;
using Common.Domain.Primitives;

namespace AquaStore.Domain.ValueObjects;

/// <summary>
/// URL-friendly идентификатор
/// </summary>
public sealed partial class Slug : ValueObject
{
    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Slug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Slug value is required", nameof(value));

        var slug = GenerateSlug(value);

        if (string.IsNullOrEmpty(slug))
            throw new ArgumentException("Could not generate valid slug", nameof(value));

        return new Slug(slug);
    }

    public static Slug FromExisting(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required", nameof(slug));

        if (!SlugValidationRegex().IsMatch(slug))
            throw new ArgumentException("Invalid slug format", nameof(slug));

        return new Slug(slug);
    }

    private static string GenerateSlug(string input)
    {
        // Транслитерация кириллицы
        var transliterated = Transliterate(input);

        // Приводим к нижнему регистру
        var slug = transliterated.ToLowerInvariant();

        // Заменяем пробелы и недопустимые символы на дефисы
        slug = InvalidCharsRegex().Replace(slug, "-");

        // Удаляем множественные дефисы
        slug = MultipleDashesRegex().Replace(slug, "-");

        // Удаляем дефисы в начале и конце
        slug = slug.Trim('-');

        return slug;
    }

    private static string Transliterate(string input)
    {
        var map = new Dictionary<char, string>
        {
            {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"},
            {'е', "e"}, {'ё', "yo"}, {'ж', "zh"}, {'з', "z"}, {'и', "i"},
            {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"}, {'н', "n"},
            {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"},
            {'у', "u"}, {'ф', "f"}, {'х', "h"}, {'ц', "ts"}, {'ч', "ch"},
            {'ш', "sh"}, {'щ', "sch"}, {'ъ', ""}, {'ы', "y"}, {'ь', ""},
            {'э', "e"}, {'ю', "yu"}, {'я', "ya"}
        };

        var sb = new StringBuilder();
        foreach (var c in input)
        {
            var lower = char.ToLowerInvariant(c);
            if (map.TryGetValue(lower, out var replacement))
            {
                sb.Append(char.IsUpper(c) ? replacement.ToUpperInvariant() : replacement);
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Slug slug) => slug.Value;

    [GeneratedRegex(@"[^a-z0-9\-]", RegexOptions.Compiled)]
    private static partial Regex InvalidCharsRegex();

    [GeneratedRegex(@"-+", RegexOptions.Compiled)]
    private static partial Regex MultipleDashesRegex();

    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled)]
    private static partial Regex SlugValidationRegex();
}

