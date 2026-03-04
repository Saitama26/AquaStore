using Common.Domain.Primitives;
using AquaStore.Domain.ValueObjects;

namespace AquaStore.Domain.Categories;

/// <summary>
/// Категория товаров
/// </summary>
public sealed class Category : AggregateRoot, IAuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Slug Slug { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<Category> _subCategories = [];
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    private Category() { }

    public static Category Create(
        string name,
        string? description = null,
        Guid? parentCategoryId = null,
        string? imageUrl = null)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Slug = Slug.Create(name),
            ParentCategoryId = parentCategoryId,
            ImageUrl = imageUrl,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        return category;
    }

    public void Update(string name, string? description, string? imageUrl)
    {
        Name = name;
        Description = description;
        Slug = Slug.Create(name);
        ImageUrl = imageUrl;
    }

    public void SetParent(Guid? parentCategoryId)
    {
        ParentCategoryId = parentCategoryId;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}

