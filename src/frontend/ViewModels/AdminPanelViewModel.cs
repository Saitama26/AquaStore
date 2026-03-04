namespace frontend.ViewModels;

public class AdminPanelViewModel
{
    public PagedResponse<ProductViewModel> Products { get; set; } = new();
    public IReadOnlyList<CategoryViewModel> Categories { get; set; } = Array.Empty<CategoryViewModel>();
    public IReadOnlyList<BrandViewModel> Brands { get; set; } = Array.Empty<BrandViewModel>();
    public PagedResponse<AdminOrderListItemViewModel> Orders { get; set; } = new();
    public IReadOnlyList<AdminUserViewModel> Users { get; set; } = Array.Empty<AdminUserViewModel>();
    public string ActiveTab { get; set; } = "products";

    // Pagination helpers
    public int ProductsPage { get; set; } = 1;
    public int ProductsPageSize { get; set; } = 50;
    public int ProductsTotalPages => Products.TotalCount > 0
        ? (int)Math.Ceiling((double)Products.TotalCount / ProductsPageSize)
        : 1;

    public int OrdersPage { get; set; } = 1;
    public int OrdersPageSize { get; set; } = 50;
    public int OrdersTotalPages => Orders.TotalCount > 0
        ? (int)Math.Ceiling((double)Orders.TotalCount / OrdersPageSize)
        : 1;
}

