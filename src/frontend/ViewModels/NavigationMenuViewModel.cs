namespace frontend.ViewModels;

public class NavigationMenuViewModel
{
    public IReadOnlyList<CategoryViewModel> Categories { get; set; } = Array.Empty<CategoryViewModel>();
    public IReadOnlyList<BrandViewModel> Brands { get; set; } = Array.Empty<BrandViewModel>();
}

