using frontend.ViewModels;

namespace frontend.Services;

/// <summary>
/// Интерфейс для работы с Backend API
/// </summary>
public interface IApiService
{
    // Products
    Task<PagedResponse<ProductViewModel>?> GetProductsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        Guid? categoryId = null,
        Guid? brandId = null,
        int? filterType = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);
    Task<ProductViewModel?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductViewModel?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default);

    // Navigation
    Task<IReadOnlyList<CategoryViewModel>?> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BrandViewModel>?> GetBrandsAsync(CancellationToken cancellationToken = default);

    // Admin
    Task<bool> CreateCategoryAsync(AdminCategoryCreateViewModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> CreateBrandAsync(AdminBrandCreateViewModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteBrandAsync(Guid brandId, CancellationToken cancellationToken = default);
    Task<bool> UpdateCategoryAsync(Guid categoryId, string name, string? description, string? imageUrl, CancellationToken cancellationToken = default);
    Task<bool> UpdateBrandAsync(Guid brandId, string name, string? description, string? country, string? logoUrl, string? websiteUrl, CancellationToken cancellationToken = default);
    Task<bool> CreateProductAsync(AdminProductCreateViewModel model, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductAsync(AdminProductUpdateViewModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> BulkImportCategoriesAsync(IReadOnlyList<AdminBulkCategoryImportItemViewModel> categories, CancellationToken cancellationToken = default);
    Task<bool> BulkImportBrandsAsync(IReadOnlyList<AdminBulkBrandImportItemViewModel> brands, CancellationToken cancellationToken = default);
    Task<bool> BulkImportProductsAsync(IReadOnlyList<AdminBulkProductImportItemViewModel> products, CancellationToken cancellationToken = default);

    // Users (admin)
    Task<PagedResponse<AdminUserViewModel>> GetUsersAsync(
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(AdminUserViewModel model, CancellationToken cancellationToken = default);

    // Cart
    Task<ApiOperationResult> AddToCartAsync(Guid productId, int quantity = 1, CancellationToken cancellationToken = default);
    Task<ApiOperationResult> UpdateCartItemQuantityAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromCartAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<CartViewModel?> GetCartAsync(CancellationToken cancellationToken = default);

    // Orders
    Task<ApiOperationResult<CreateOrderResponseViewModel>> CreateOrderAsync(
        CreateOrderViewModel model,
        CancellationToken cancellationToken = default);
    Task<PagedResponse<OrderListItemViewModel>> GetOrdersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
    Task<OrderDetailViewModel?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<(byte[]? Content, string? FileName, string? ContentType)> DownloadOrderReceiptPdfAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderStatusAsync(
        Guid orderId,
        int status,
        CancellationToken cancellationToken = default);
    Task<PagedResponse<AdminOrderListItemViewModel>> GetAllOrdersAsync(
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
    Task<AdminOrderAnalyticsViewModel?> GetOrderAnalyticsAsync(
        int topProducts = 10,
        int topUsers = 10,
        CancellationToken cancellationToken = default);
    Task<(byte[]? Content, string? FileName, string? ContentType)> ExportOrdersCsvAsync(
        CancellationToken cancellationToken = default);
    Task<(byte[]? Content, string? FileName, string? ContentType)> ExportOrderAnalyticsCsvAsync(
        CancellationToken cancellationToken = default);
        
    // Auth (если нужно)
    Task<AuthResponseViewModel?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<ApiOperationResult> RegisterAsync(
        string email,
        string password,
        string confirmPassword,
        string firstName,
        string lastName,
        string phone,
        CancellationToken cancellationToken = default);
    Task<ApiOperationResult> ConfirmRegistrationAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default);

    // Profile
    Task<UserProfileViewModel?> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<ApiOperationResult> AddAddressAsync(AddAddressViewModel model, CancellationToken cancellationToken = default);
    Task<ApiOperationResult> UpdateProfileAsync(
        string firstName,
        string lastName,
        string? phone,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<ReviewViewModel>> GetReviewsByProductIdAsync(
        Guid productId,
        int pageNumber = 1,
        int pageSize = 5,
        CancellationToken cancellationToken = default);
    Task<ApiOperationResult> CreateReviewAsync(
        Guid productId,
        int rating,
        string? comment,
        CancellationToken cancellationToken = default);
}

