using Common.Domain.Primitives;

namespace AquaStore.Domain.Products;

/// <summary>
/// Изображение товара
/// </summary>
public sealed class ProductImage : Entity
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = null!;
    public string? AltText { get; private set; }
    public bool IsMain { get; private set; }
    public int SortOrder { get; private set; }

    private ProductImage() { }

    internal static ProductImage Create(
        Guid productId,
        string url,
        bool isMain = false,
        int sortOrder = 0,
        string? altText = null)
    {
        return new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Url = url,
            IsMain = isMain,
            SortOrder = sortOrder,
            AltText = altText
        };
    }

    public void SetAsMain()
    {
        IsMain = true;
    }

    public void SetAsSecondary()
    {
        IsMain = false;
    }

    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
    }
}

