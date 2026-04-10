namespace frontend.ViewModels;

public sealed class AdminBulkCategoryImportItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}

public sealed class AdminBulkBrandImportItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
}

public sealed class AdminBulkProductImportItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int FilterType { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string? Sku { get; set; }
    public int? FilterLifespanMonths { get; set; }
    public int? FilterCapacityLiters { get; set; }
    public double? FlowRateLitersPerMinute { get; set; }
    public List<string>? ImageUrls { get; set; }
}
