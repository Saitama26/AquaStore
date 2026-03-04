using Common.Domain.Primitives;
using Common.Domain.Events;
using AquaStore.Domain.Enums;
using AquaStore.Domain.ValueObjects;
using AquaStore.Domain.Categories;
using AquaStore.Domain.Brands;
using AquaStore.Domain.Reviews;

namespace AquaStore.Domain.Products;

/// <summary>
/// Товар - фильтр для воды
/// </summary>
public sealed class Product : AggregateRoot, IAuditableEntity
{
    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string? ShortDescription { get; private set; }
    public Money Price { get; private set; } = null!;
    public Money? OldPrice { get; private set; }
    public FilterType FilterType { get; private set; }
    public int StockQuantity { get; private set; }
    public string? Sku { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsFeatured { get; private set; }

    // Характеристики фильтра
    public int? FilterLifespanMonths { get; private set; }
    public int? FilterCapacityLiters { get; private set; }
    public double? FlowRateLitersPerMinute { get; private set; }

    // Связи
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    public Guid BrandId { get; private set; }
    public Brand Brand { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<ProductImage> _images = [];
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private readonly List<Review> _reviews = [];
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    private Product() { }

    public static Product Create(
        string name,
        string description,
        decimal price,
        FilterType filterType,
        Guid categoryId,
        Guid brandId,
        int stockQuantity = 0,
        string? sku = null,
        string? shortDescription = null)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = Slug.Create(name),
            Description = description,
            ShortDescription = shortDescription,
            Price = Money.Create(price),
            FilterType = filterType,
            CategoryId = categoryId,
            BrandId = brandId,
            StockQuantity = stockQuantity,
            Sku = sku,
            IsActive = true,
            IsFeatured = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        product.RaiseDomainEvent(new ProductCreatedEvent(product.Id, product.Name));

        return product;
    }

    public void Update(
        string name,
        string description,
        string? shortDescription,
        decimal price,
        FilterType filterType,
        Guid categoryId,
        Guid brandId,
        string? sku)
    {
        Name = name;
        Slug = Slug.Create(name);
        Description = description;
        ShortDescription = shortDescription;
        Price = Money.Create(price);
        FilterType = filterType;
        CategoryId = categoryId;
        BrandId = brandId;
        Sku = sku;
    }

    public void SetSpecifications(
        int? filterLifespanMonths,
        int? filterCapacityLiters,
        double? flowRateLitersPerMinute)
    {
        FilterLifespanMonths = filterLifespanMonths;
        FilterCapacityLiters = filterCapacityLiters;
        FlowRateLitersPerMinute = flowRateLitersPerMinute;
    }

    public void SetOldPrice(decimal? oldPrice)
    {
        OldPrice = oldPrice.HasValue ? Money.Create(oldPrice.Value) : null;
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));

        var previousQuantity = StockQuantity;
        StockQuantity = quantity;

        RaiseDomainEvent(new ProductStockChangedEvent(Id, previousQuantity, quantity));
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        UpdateStock(StockQuantity + quantity);
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (StockQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock");

        UpdateStock(StockQuantity - quantity);
    }

    public bool IsInStock => StockQuantity > 0;

    public void AddImage(string url, bool isMain = false, int sortOrder = 0)
    {
        if (isMain)
        {
            foreach (var img in _images.Where(i => i.IsMain))
            {
                img.SetAsSecondary();
            }
        }

        var image = ProductImage.Create(Id, url, isMain, sortOrder);
        _images.Add(image);
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is not null)
        {
            _images.Remove(image);
        }
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void SetFeatured(bool isFeatured) => IsFeatured = isFeatured;

    public double? AverageRating => _reviews.Count > 0
        ? _reviews.Average(r => r.Rating.Value)
        : null;
}

/// <summary>
/// Событие создания товара
/// </summary>
public sealed record ProductCreatedEvent(Guid ProductId, string ProductName) : DomainEvent;

/// <summary>
/// Событие изменения остатков товара
/// </summary>
public sealed record ProductStockChangedEvent(Guid ProductId, int PreviousQuantity, int NewQuantity) : DomainEvent;

