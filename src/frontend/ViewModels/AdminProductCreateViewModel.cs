namespace frontend.ViewModels;

public class AdminProductCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int FilterType { get; set; }
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public int StockQuantity { get; set; }
    public string? Sku { get; set; }
    public int? FilterLifespanMonths { get; set; }
    public int? FilterCapacityLiters { get; set; }
    public double? FlowRateLitersPerMinute { get; set; }
    public string ImageUrls { get; set; } = string.Empty;
}

