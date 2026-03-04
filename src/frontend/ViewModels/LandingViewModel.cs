namespace frontend.ViewModels;

public class LandingViewModel
{
    public IReadOnlyList<CategoryViewModel> Categories { get; set; } = Array.Empty<CategoryViewModel>();
    public IReadOnlyList<BrandViewModel> Brands { get; set; } = Array.Empty<BrandViewModel>();
    public List<ProductViewModel> FeaturedProducts { get; set; } = new();
    public List<ProductViewModel> NewProducts { get; set; } = new();
    public int TotalProductsCount { get; set; }
}

