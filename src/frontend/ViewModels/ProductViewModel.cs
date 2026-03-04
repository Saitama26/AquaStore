using System.Linq;

namespace frontend.ViewModels;

/// <summary>
/// Модель товара для представлений (соответствует ProductResponse из API)
/// </summary>
public class ProductViewModel
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public string? MainImageUrl
    {
        get => _mainImageUrl ?? Images?.FirstOrDefault(i => i.IsMain)?.Url
            ?? Images?.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url;
        set => _mainImageUrl = value;
    }
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public int FilterType { get; set; }
    public string FilterTypeName { get; set; } = string.Empty;
    public bool IsInStock { get; set; }
    public bool IsFeatured { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string Currency { get; set; } = "BYN";
    public string? Sku { get; set; }
    public List<ProductImageViewModel> Images { get; set; } = new();

    private string? _mainImageUrl;
}

public class ProductDetailsViewModel
{
    public ProductViewModel Product { get; set; } = new();
    public List<ProductViewModel> SimilarProducts { get; set; } = new();
    public PagedResponse<ReviewViewModel> Reviews { get; set; } = new();
    public ReviewCreateViewModel NewReview { get; set; } = new();
    public int ReviewsPageSize { get; set; } = 5;
}

public class ProductImageViewModel
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }
}
