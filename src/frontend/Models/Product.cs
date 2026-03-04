namespace frontend.Models;

/// <summary>
/// Модель товара для представлений
/// </summary>
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public string Currency { get; set; } = "BYN";
    
    // Тип фильтра
    public int FilterType { get; set; }
    public string FilterTypeName { get; set; } = string.Empty;
    
    // Остатки и статусы
    public int StockQuantity { get; set; }
    public bool IsInStock => StockQuantity > 0;
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string? Sku { get; set; }
    
    // Характеристики фильтра
    public int? FilterLifespanMonths { get; set; }
    public int? FilterCapacityLiters { get; set; }
    public double? FlowRateLitersPerMinute { get; set; }
    
    // Связи
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    
    // Изображения
    public List<ProductImage> Images { get; set; } = new();
    public string? MainImageUrl => Images.FirstOrDefault(i => i.IsMain)?.Url 
        ?? Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url;
    
    // Отзывы и рейтинг
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    
    // Даты
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
