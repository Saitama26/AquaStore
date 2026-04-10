using System.Net.Http.Json;
using System.Text.Json;
using frontend.ViewModels;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace frontend.Services;

/// <summary>
/// Сервис для работы с Backend API
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiSettings _settings;
    private readonly ILogger<ApiService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(
        IHttpClientFactory httpClientFactory,
        IOptions<ApiSettings> settings,
        ILogger<ApiService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _settings = settings.Value;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private void AddAuthHeader(HttpRequestMessage request)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["access_token"];
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    // Products
    public async Task<PagedResponse<ProductViewModel>?> GetProductsAsync(
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
            if (categoryId.HasValue)
                query.Add($"categoryId={categoryId}");
            if (brandId.HasValue)
                query.Add($"brandId={brandId}");
            if (filterType.HasValue)
                query.Add($"filterType={filterType}");
            if (minPrice.HasValue)
                query.Add($"minPrice={minPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            if (maxPrice.HasValue)
                query.Add($"maxPrice={maxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            if (inStock.HasValue)
                query.Add($"inStock={inStock.Value.ToString().ToLowerInvariant()}");
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
                query.Add($"sortDescending={sortDescending.ToString().ToLowerInvariant()}");
            }

            var response = await _httpClient.GetAsync(
                $"{_settings.BaseUrl}/api/products?{string.Join("&", query)}",
                cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ProductViewModel>>>(_jsonOptions, cancellationToken);
            
            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }
            
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка товаров");
            return null;
        }
    }

    // Navigation
    public async Task<IReadOnlyList<CategoryViewModel>?> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/api/categories", cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<List<CategoryViewModel>>>(_jsonOptions, cancellationToken);

            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }

            return Array.Empty<CategoryViewModel>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении категорий");
            return Array.Empty<CategoryViewModel>();
        }
    }

    public async Task<IReadOnlyList<BrandViewModel>?> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/api/brands", cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<List<BrandViewModel>>>(_jsonOptions, cancellationToken);

            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }

            return Array.Empty<BrandViewModel>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении брендов");
            return Array.Empty<BrandViewModel>();
        }
    }

    // Admin
    public async Task<bool> CreateCategoryAsync(AdminCategoryCreateViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                name = model.Name,
                description = model.Description,
                parentCategoryId = model.ParentCategoryId,
                imageUrl = model.ImageUrl
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/api/categories")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при создании категории");
            return false;
        }
    }

    public async Task<bool> DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_settings.BaseUrl}/api/categories/{categoryId}");
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при удалении категории: {CategoryId}", categoryId);
            return false;
        }
    }

    public async Task<bool> CreateBrandAsync(AdminBrandCreateViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                name = model.Name,
                description = model.Description,
                country = model.Country,
                logoUrl = model.LogoUrl,
                websiteUrl = model.WebsiteUrl
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/api/brands")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при создании бренда");
            return false;
        }
    }

    public async Task<bool> BulkImportCategoriesAsync(
        IReadOnlyList<AdminBulkCategoryImportItemViewModel> categories,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                categories = categories.Select(c => new
                {
                    name = c.Name,
                    description = c.Description,
                    imageUrl = c.ImageUrl
                }).ToList()
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/api/categories/bulk")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при массовом импорте категорий");
            return false;
        }
    }

    public async Task<bool> BulkImportBrandsAsync(
        IReadOnlyList<AdminBulkBrandImportItemViewModel> brands,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                brands = brands.Select(b => new
                {
                    name = b.Name,
                    description = b.Description,
                    country = b.Country,
                    logoUrl = b.LogoUrl,
                    websiteUrl = b.WebsiteUrl
                }).ToList()
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/api/brands/bulk")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при массовом импорте брендов");
            return false;
        }
    }

    public async Task<bool> BulkImportProductsAsync(
        IReadOnlyList<AdminBulkProductImportItemViewModel> products,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                products = products.Select(p => new
                {
                    name = p.Name,
                    description = p.Description,
                    shortDescription = p.ShortDescription,
                    price = p.Price,
                    oldPrice = p.OldPrice,
                    filterType = p.FilterType,
                    categoryName = p.CategoryName,
                    brandName = p.BrandName,
                    stockQuantity = p.StockQuantity,
                    sku = p.Sku,
                    filterLifespanMonths = p.FilterLifespanMonths,
                    filterCapacityLiters = p.FilterCapacityLiters,
                    flowRateLitersPerMinute = p.FlowRateLitersPerMinute,
                    imageUrls = p.ImageUrls
                }).ToList()
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/api/products/bulk")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при массовом импорте товаров");
            return false;
        }
    }

    public async Task<bool> DeleteBrandAsync(Guid brandId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_settings.BaseUrl}/api/brands/{brandId}");
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при удалении бренда: {BrandId}", brandId);
            return false;
        }
    }

    public async Task<PagedResponse<AdminUserViewModel>> GetUsersAsync(
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.BaseUrl}/api/users?pageNumber={pageNumber}&pageSize={pageSize}");
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<PagedResponse<AdminUserViewModel>>>(_jsonOptions, cancellationToken);

            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }

            return PagedResultEmpty(pageNumber, pageSize);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка пользователей");
            return PagedResultEmpty(pageNumber, pageSize);
        }

        static PagedResponse<AdminUserViewModel> PagedResultEmpty(int page, int size) =>
            new()
            {
                Items = new List<AdminUserViewModel>(),
                TotalCount = 0,
                PageNumber = page,
                PageSize = size
            };
    }

    public async Task<bool> UpdateUserAsync(AdminUserViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                firstName = model.FirstName,
                lastName = model.LastName,
                phone = model.Phone,
                role = model.Role,
                isActive = model.IsActive
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"{_settings.BaseUrl}/api/users/{model.Id}")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении пользователя {UserId}", model.Id);
            return false;
        }
    }

    public async Task<bool> UpdateCategoryAsync(Guid categoryId, string name, string? description, string? imageUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new { name, description, imageUrl };
            using var request = new HttpRequestMessage(HttpMethod.Put, $"{_settings.BaseUrl}/api/categories/{categoryId}")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении категории: {CategoryId}", categoryId);
            return false;
        }
    }

    public async Task<bool> UpdateBrandAsync(Guid brandId, string name, string? description, string? country, string? logoUrl, string? websiteUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new { name, description, country, logoUrl, websiteUrl };
            using var request = new HttpRequestMessage(HttpMethod.Put, $"{_settings.BaseUrl}/api/brands/{brandId}")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении бренда: {BrandId}", brandId);
            return false;
        }
    }

    public async Task<bool> CreateProductAsync(AdminProductCreateViewModel model, CancellationToken cancellationToken = default)
    {
        if (model is null)
        {
            _logger.LogWarning("Не удалось создать товар: модель формы равна null.");
            return false;
        }

        try
        {
            var imageUrls = (model.ImageUrls ?? string.Empty)
                .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(url => url.Trim())
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .ToList();

            var payload = new
            {
                name = model.Name,
                description = model.Description,
                shortDescription = model.ShortDescription,
                price = model.Price,
                oldPrice = model.OldPrice,
                filterType = model.FilterType,
                categoryId = model.CategoryId,
                brandId = model.BrandId,
                stockQuantity = model.StockQuantity,
                sku = model.Sku,
                filterLifespanMonths = model.FilterLifespanMonths,
                filterCapacityLiters = model.FilterCapacityLiters,
                flowRateLitersPerMinute = model.FlowRateLitersPerMinute,
                imageUrls
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/api/products")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при создании товара");
            return false;
        }
    }

    public async Task<bool> UpdateProductAsync(AdminProductUpdateViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                name = model.Name,
                description = model.Description,
                shortDescription = model.ShortDescription,
                price = model.Price,
                oldPrice = model.OldPrice,
                filterType = model.FilterType,
                categoryId = model.CategoryId,
                brandId = model.BrandId,
                sku = model.Sku,
                filterLifespanMonths = model.FilterLifespanMonths,
                filterCapacityLiters = model.FilterCapacityLiters,
                flowRateLitersPerMinute = model.FlowRateLitersPerMinute
            };

            using var request = new HttpRequestMessage(HttpMethod.Put, $"{_settings.BaseUrl}/api/products/{model.ProductId}")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении товара: {ProductId}", model.ProductId);
            return false;
        }
    }

    public async Task<bool> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_settings.BaseUrl}/api/products/{productId}");
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при удалении товара: {ProductId}", productId);
            return false;
        }
    }

    public async Task<ProductViewModel?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/api/products/{id}", cancellationToken);
            response.EnsureSuccessStatusCode();
            
            // API возвращает ApiResponse<ProductDetailResponse>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProductViewModel>>(_jsonOptions, cancellationToken);
            
            if (apiResponse?.Success == true)
            {
                return apiResponse.Data;
            }
            
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении товара по ID: {ProductId}", id);
            return null;
        }
    }

    public async Task<ProductViewModel?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/api/products/by-slug/{slug}", cancellationToken);
            response.EnsureSuccessStatusCode();
            
            // API возвращает ApiResponse<ProductDetailResponse>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProductViewModel>>(_jsonOptions, cancellationToken);
            
            if (apiResponse?.Success == true)
            {
                return apiResponse.Data;
            }
            
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении товара по slug: {Slug}", slug);
            return null;
        }
    }

    // Cart
    public async Task<ApiOperationResult> AddToCartAsync(Guid productId, int quantity = 1, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { ProductId = productId, Quantity = quantity };
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/api/cart/items")
            {
                Content = JsonContent.Create(request)
            };
            AddAuthHeader(httpRequest);
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            ApiResponse<object>? apiResponse = null;
            if (response.Content.Headers.ContentLength is > 0)
            {
                apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions, cancellationToken);
            }

            return new ApiOperationResult(
                response.IsSuccessStatusCode,
                apiResponse?.Message ?? (response.IsSuccessStatusCode ? null : "Не удалось добавить товар в корзину."),
                apiResponse?.Errors);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении товара в корзину: {ProductId}", productId);
            return ApiOperationResult.Fail("Ошибка сети при добавлении товара в корзину.");
        }
    }

    public async Task<bool> UpdateProductStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new { quantity };
            using var request = new HttpRequestMessage(HttpMethod.Patch, $"{_settings.BaseUrl}/api/products/{productId}/stock")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении остатка товара: {ProductId}", productId);
            return false;
        }
    }

    public async Task<ApiOperationResult> UpdateCartItemQuantityAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { Quantity = quantity };
            var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"{_settings.BaseUrl}/api/cart/items/{productId}")
            {
                Content = JsonContent.Create(request)
            };
            AddAuthHeader(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            ApiResponse<object>? apiResponse = null;
            if (response.Content.Headers.ContentLength is > 0)
            {
                apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions, cancellationToken);
            }

            return new ApiOperationResult(
                response.IsSuccessStatusCode,
                apiResponse?.Message ?? (response.IsSuccessStatusCode ? null : "Не удалось обновить количество товара."),
                apiResponse?.Errors);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении количества товара в корзине: {ProductId}", productId);
            return ApiOperationResult.Fail("Ошибка сети при обновлении количества товара.");
        }
    }

    public async Task<bool> RemoveFromCartAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{_settings.BaseUrl}/api/cart/items/{productId}");
            AddAuthHeader(httpRequest);
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            // API возвращает ApiResponse
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions, cancellationToken);
            return apiResponse?.Success == true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при удалении товара из корзины: {ProductId}", productId);
            return false;
        }
    }

    public async Task<CartViewModel?> GetCartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_settings.BaseUrl}/api/cart");
            AddAuthHeader(httpRequest);
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<CartViewModel>>(_jsonOptions, cancellationToken);
            if (apiResponse?.Success == true)
            {
                return apiResponse.Data;
            }

            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении корзины");
            return null;
        }
    }

    // Orders
    public async Task<ApiOperationResult<CreateOrderResponseViewModel>> CreateOrderAsync(
        CreateOrderViewModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                city = model.City,
                street = model.Street,
                building = model.Building,
                apartment = model.Apartment,
                postalCode = model.PostalCode,
                customerNote = model.CustomerNote,
                productIds = model.SelectedProductIds,
                buyNowSingleUnit = model.BuyNowSingleUnit
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/api/orders")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            ApiResponse<CreateOrderResponseViewModel>? apiResponse = null;
            if (response.Content.Headers.ContentLength is > 0)
            {
                apiResponse = await response.Content
                    .ReadFromJsonAsync<ApiResponse<CreateOrderResponseViewModel>>(_jsonOptions, cancellationToken);
            }

            return new ApiOperationResult<CreateOrderResponseViewModel>(
                response.IsSuccessStatusCode,
                apiResponse?.Data,
                apiResponse?.Message ?? (response.IsSuccessStatusCode ? null : "Не удалось оформить заказ."),
                apiResponse?.Errors);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при оформлении заказа");
            return ApiOperationResult<CreateOrderResponseViewModel>.Fail("Ошибка сети при оформлении заказа.");
        }
    }

    public async Task<PagedResponse<OrderListItemViewModel>> GetOrdersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.BaseUrl}/api/orders?pageNumber={pageNumber}&pageSize={pageSize}");
            AddAuthHeader(httpRequest);
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<PagedResponse<OrderListItemViewModel>>>(_jsonOptions, cancellationToken);

            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }

            return new PagedResponse<OrderListItemViewModel>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении заказов");
            return new PagedResponse<OrderListItemViewModel>();
        }
    }

    public async Task<PagedResponse<AdminOrderListItemViewModel>> GetAllOrdersAsync(
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.BaseUrl}/api/orders/all?pageNumber={pageNumber}&pageSize={pageSize}");
            AddAuthHeader(httpRequest);
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<PagedResponse<AdminOrderListItemViewModel>>>(_jsonOptions, cancellationToken);

            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }

            return new PagedResponse<AdminOrderListItemViewModel>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении всех заказов");
            return new PagedResponse<AdminOrderListItemViewModel>();
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(
        Guid orderId,
        int status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new { status };
            using var request = new HttpRequestMessage(HttpMethod.Patch, $"{_settings.BaseUrl}/api/orders/{orderId}/status")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении статуса заказа: {OrderId}", orderId);
            return false;
        }
    }

    public async Task<OrderDetailViewModel?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_settings.BaseUrl}/api/orders/{orderId}");
            AddAuthHeader(httpRequest);
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<OrderDetailViewModel>>(_jsonOptions, cancellationToken);

            return apiResponse?.Success == true ? apiResponse.Data : null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении заказа: {OrderId}", orderId);
            return null;
        }
    }

    public async Task<(byte[]? Content, string? FileName, string? ContentType)> DownloadOrderReceiptPdfAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.BaseUrl}/api/orders/{orderId}/receipt/pdf");
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Не удалось скачать PDF-чек заказа {OrderId}. StatusCode: {StatusCode}", orderId, response.StatusCode);
                return (null, null, null);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf";
            var contentDisposition = response.Content.Headers.ContentDisposition;
            var fileName = contentDisposition?.FileNameStar ?? contentDisposition?.FileName;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                fileName = fileName.Trim('"');
            }

            return (bytes, fileName, contentType);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Ошибка при скачивании PDF-чека заказа: {OrderId}", orderId);
            return (null, null, null);
        }
    }

    public async Task<AdminOrderAnalyticsViewModel?> GetOrderAnalyticsAsync(
        int topProducts = 10,
        int topUsers = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.BaseUrl}/api/orders/analytics?topProducts={topProducts}&topUsers={topUsers}");
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<AdminOrderAnalyticsViewModel>>(_jsonOptions, cancellationToken);

            return apiResponse?.Success == true ? apiResponse.Data : null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении аналитики заказов");
            return null;
        }
    }

    public async Task<(byte[]? Content, string? FileName, string? ContentType)> ExportOrdersCsvAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.BaseUrl}/api/orders/export/csv");
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Не удалось экспортировать заказы в CSV. StatusCode: {StatusCode}", response.StatusCode);
                return (null, null, null);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "text/csv";
            var contentDisposition = response.Content.Headers.ContentDisposition;
            var fileName = contentDisposition?.FileNameStar ?? contentDisposition?.FileName;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                fileName = fileName.Trim('"');
            }

            return (bytes, fileName, contentType);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Ошибка при экспорте заказов в CSV");
            return (null, null, null);
        }
    }

    public async Task<(byte[]? Content, string? FileName, string? ContentType)> ExportOrderAnalyticsCsvAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.BaseUrl}/api/orders/analytics/export/csv");
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Не удалось экспортировать расширенную аналитику в CSV. StatusCode: {StatusCode}", response.StatusCode);
                return (null, null, null);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "text/csv";
            var contentDisposition = response.Content.Headers.ContentDisposition;
            var fileName = contentDisposition?.FileNameStar ?? contentDisposition?.FileName;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                fileName = fileName.Trim('"');
            }

            return (bytes, fileName, contentType);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Ошибка при экспорте расширенной аналитики в CSV");
            return (null, null, null);
        }
    }

    // Auth
    public async Task<AuthResponseViewModel?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync($"{_settings.BaseUrl}/api/auth/login", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseViewModel>>(_jsonOptions, cancellationToken);
            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }

            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при входе");
            return null;
        }
    }

    public async Task<ApiOperationResult> RegisterAsync(
        string email,
        string password,
        string confirmPassword,
        string firstName,
        string lastName,
        string phone,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone
            };
            var response = await _httpClient.PostAsJsonAsync($"{_settings.BaseUrl}/api/auth/register", request, cancellationToken);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions, cancellationToken);

            return new ApiOperationResult(
                response.IsSuccessStatusCode,
                apiResponse?.Message,
                apiResponse?.Errors);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации");
            return ApiOperationResult.Fail("Ошибка сети при регистрации.");
        }
    }

    public async Task<ApiOperationResult> ConfirmRegistrationAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { Email = email, Code = code };
            var response = await _httpClient.PostAsJsonAsync($"{_settings.BaseUrl}/api/auth/register/confirm", request, cancellationToken);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions, cancellationToken);

            return new ApiOperationResult(
                response.IsSuccessStatusCode,
                apiResponse?.Message,
                apiResponse?.Errors);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при подтверждении регистрации");
            return ApiOperationResult.Fail("Ошибка сети при подтверждении регистрации.");
        }
    }

    // Profile
    public async Task<UserProfileViewModel?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_settings.BaseUrl}/api/auth/profile");
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<UserProfileViewModel>>(_jsonOptions, cancellationToken);

            return apiResponse?.Success == true ? apiResponse.Data : null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении профиля");
            return null;
        }
    }

    public async Task<ApiOperationResult> AddAddressAsync(AddAddressViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                city = model.City,
                street = model.Street,
                building = model.Building,
                apartment = model.Apartment,
                postalCode = model.PostalCode,
                isDefault = model.IsDefault
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/api/auth/profile/addresses")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions, cancellationToken);

            return new ApiOperationResult(
                response.IsSuccessStatusCode,
                apiResponse?.Message,
                apiResponse?.Errors);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении адреса пользователя");
            return ApiOperationResult.Fail("Ошибка сети при добавлении адреса.");
        }
    }

    public async Task<ApiOperationResult> UpdateProfileAsync(
        string firstName,
        string lastName,
        string? phone,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new { firstName, lastName, phone };
            using var request = new HttpRequestMessage(HttpMethod.Put, $"{_settings.BaseUrl}/api/auth/profile")
            {
                Content = JsonContent.Create(payload)
            };
            AddAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions, cancellationToken);

            return new ApiOperationResult(
                response.IsSuccessStatusCode,
                apiResponse?.Message,
                apiResponse?.Errors);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении профиля");
            return ApiOperationResult.Fail("Ошибка сети при обновлении профиля.");
        }
    }

    // Reviews
    public async Task<PagedResponse<ReviewViewModel>> GetReviewsByProductIdAsync(
        Guid productId,
        int pageNumber = 1,
        int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_settings.BaseUrl}/api/reviews/product/{productId}?pageNumber={pageNumber}&pageSize={pageSize}",
                cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<PagedResponse<ReviewViewModel>>>(_jsonOptions, cancellationToken);

            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }

            return new PagedResponse<ReviewViewModel>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при получении отзывов");
            return new PagedResponse<ReviewViewModel>();
        }
    }

    public async Task<ApiOperationResult> CreateReviewAsync(
        Guid productId,
        int rating,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { Rating = rating, Comment = comment };
            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.BaseUrl}/api/reviews/product/{productId}")
            {
                Content = JsonContent.Create(request)
            };
            AddAuthHeader(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            ApiResponse<object>? apiResponse = null;
            if (response.Content.Headers.ContentLength is > 0)
            {
                apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions, cancellationToken);
            }

            return new ApiOperationResult(
                response.IsSuccessStatusCode,
                apiResponse?.Message ?? (response.IsSuccessStatusCode ? null : "Не удалось отправить отзыв."),
                apiResponse?.Errors);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении отзыва");
            return ApiOperationResult.Fail("Ошибка сети при добавлении отзыва.");
        }
    }
}

/// <summary>
/// Настройки API
/// </summary>
public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}

