using Common.Domain.Primitives;
using AquaStore.Domain.ValueObjects;

namespace AquaStore.Domain.Brands;

/// <summary>
/// Бренд / Производитель
/// </summary>
public sealed class Brand : AggregateRoot, IAuditableEntity
{
    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Country { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private Brand() { }

    public static Brand Create(
        string name,
        string? description = null,
        string? country = null,
        string? logoUrl = null,
        string? websiteUrl = null)
    {
        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = Slug.Create(name),
            Description = description,
            Country = country,
            LogoUrl = logoUrl,
            WebsiteUrl = websiteUrl,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        return brand;
    }

    public void Update(
        string name,
        string? description,
        string? country,
        string? logoUrl,
        string? websiteUrl)
    {
        Name = name;
        Slug = Slug.Create(name);
        Description = description;
        Country = country;
        LogoUrl = logoUrl;
        WebsiteUrl = websiteUrl;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}

