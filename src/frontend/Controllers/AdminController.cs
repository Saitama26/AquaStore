using frontend.Services;
using frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace frontend.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions JsonTemplateOptions = new()
    {
        WriteIndented = true
    };

    private readonly IApiService _apiService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IApiService apiService, ILogger<AdminController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [HttpGet("/admin")]
    public async Task<IActionResult> Index(
        [FromQuery] string tab = "products",
        [FromQuery] int productsPage = 1,
        [FromQuery] int ordersPage = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 50;
        productsPage = productsPage < 1 ? 1 : productsPage;
        ordersPage = ordersPage < 1 ? 1 : ordersPage;

        var model = new AdminPanelViewModel
        {
            ActiveTab = tab,
            ProductsPage = productsPage,
            ProductsPageSize = pageSize,
            OrdersPage = ordersPage,
            OrdersPageSize = pageSize
        };

        var productsTask = _apiService.GetProductsAsync(productsPage, pageSize, cancellationToken: cancellationToken);
        var categoriesTask = _apiService.GetCategoriesAsync(cancellationToken);
        var brandsTask = _apiService.GetBrandsAsync(cancellationToken);
        var ordersTask = _apiService.GetAllOrdersAsync(ordersPage, pageSize, cancellationToken);
        var usersTask = _apiService.GetUsersAsync(1, pageSize, cancellationToken);
        var analyticsTask = _apiService.GetOrderAnalyticsAsync(10, 10, cancellationToken);

        await Task.WhenAll(productsTask, categoriesTask, brandsTask, ordersTask, usersTask, analyticsTask);

        model.Products = await productsTask ?? new PagedResponse<ProductViewModel>();
        model.Categories = (await categoriesTask) ?? Array.Empty<CategoryViewModel>();
        model.Brands = (await brandsTask) ?? Array.Empty<BrandViewModel>();
        model.Orders = await ordersTask;
        var usersResponse = await usersTask;
        model.Users = usersResponse?.Items ?? new List<AdminUserViewModel>();
        model.Analytics = await analyticsTask ?? new AdminOrderAnalyticsViewModel();

        return View(model);
    }

    // ───── Categories ─────

    [HttpPost("/admin/category/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(AdminCategoryCreateViewModel model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["ToastMessage"] = "Введите название категории.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", new { tab = "categories" });
        }

        var success = await _apiService.CreateCategoryAsync(model, ct);
        TempData["ToastMessage"] = success ? "Категория добавлена." : "Не удалось добавить категорию.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "categories" });
    }

    [HttpPost("/admin/category/import-json")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportCategoriesJson(IFormFile? jsonFile, CancellationToken ct)
    {
        var items = await ParseJsonArrayFromUploadAsync<AdminBulkCategoryImportItemViewModel>(jsonFile, ct, "категорий");
        if (items is null)
        {
            return RedirectToAction("Index", new { tab = "categories" });
        }

        var success = await _apiService.BulkImportCategoriesAsync(items, ct);
        TempData["ToastMessage"] = success
            ? $"Импорт категорий завершен. Обработано: {items.Count}."
            : "Не удалось импортировать категории.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "categories" });
    }

    [HttpPost("/admin/category/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCategory(Guid categoryId, string name, string? description, string? imageUrl, CancellationToken ct)
    {
        if (categoryId == Guid.Empty || string.IsNullOrWhiteSpace(name))
        {
            TempData["ToastMessage"] = "Укажите ID и название.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", new { tab = "categories" });
        }

        var success = await _apiService.UpdateCategoryAsync(categoryId, name, description, imageUrl, ct);
        TempData["ToastMessage"] = success ? "Категория обновлена." : "Не удалось обновить категорию.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "categories" });
    }

    [HttpPost("/admin/category/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(Guid categoryId, CancellationToken ct)
    {
        if (categoryId == Guid.Empty)
        {
            TempData["ToastMessage"] = "Укажите категорию.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", new { tab = "categories" });
        }

        var success = await _apiService.DeleteCategoryAsync(categoryId, ct);
        TempData["ToastMessage"] = success ? "Категория удалена." : "Не удалось удалить категорию.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "categories" });
    }

    // ───── Brands ─────

    [HttpPost("/admin/brand/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBrand(AdminBrandCreateViewModel model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["ToastMessage"] = "Введите название бренда.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", new { tab = "brands" });
        }

        var success = await _apiService.CreateBrandAsync(model, ct);
        TempData["ToastMessage"] = success ? "Бренд добавлен." : "Не удалось добавить бренд.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "brands" });
    }

    [HttpPost("/admin/brand/import-json")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportBrandsJson(IFormFile? jsonFile, CancellationToken ct)
    {
        var items = await ParseJsonArrayFromUploadAsync<AdminBulkBrandImportItemViewModel>(jsonFile, ct, "брендов");
        if (items is null)
        {
            return RedirectToAction("Index", new { tab = "brands" });
        }

        var success = await _apiService.BulkImportBrandsAsync(items, ct);
        TempData["ToastMessage"] = success
            ? $"Импорт брендов завершен. Обработано: {items.Count}."
            : "Не удалось импортировать бренды.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "brands" });
    }

    [HttpPost("/admin/brand/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBrand(Guid brandId, string name, string? description, string? country, string? logoUrl, string? websiteUrl, CancellationToken ct)
    {
        if (brandId == Guid.Empty || string.IsNullOrWhiteSpace(name))
        {
            TempData["ToastMessage"] = "Укажите ID и название.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", new { tab = "brands" });
        }

        var success = await _apiService.UpdateBrandAsync(brandId, name, description, country, logoUrl, websiteUrl, ct);
        TempData["ToastMessage"] = success ? "Бренд обновлен." : "Не удалось обновить бренд.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "brands" });
    }

    [HttpPost("/admin/brand/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBrand(Guid brandId, CancellationToken ct)
    {
        if (brandId == Guid.Empty)
        {
            TempData["ToastMessage"] = "Укажите бренд.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", new { tab = "brands" });
        }

        var success = await _apiService.DeleteBrandAsync(brandId, ct);
        TempData["ToastMessage"] = success ? "Бренд удален." : "Не удалось удалить бренд.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "brands" });
    }

    // ───── Products ─────

    [HttpPost("/admin/product/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(AdminProductCreateViewModel model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Description))
        {
            TempData["ToastMessage"] = "Заполните название и описание товара.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", new { tab = "products" });
        }

        var success = await _apiService.CreateProductAsync(model, ct);
        TempData["ToastMessage"] = success ? "Товар добавлен." : "Не удалось добавить товар.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "products" });
    }

    [HttpPost("/admin/product/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProduct(AdminProductUpdateViewModel model, CancellationToken ct)
    {
        if (model.ProductId == Guid.Empty || string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["ToastMessage"] = "Укажите товар и название.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", new { tab = "products" });
        }

        var productUpdated = await _apiService.UpdateProductAsync(model, ct);
        var stockUpdated = productUpdated && await _apiService.UpdateProductStockAsync(model.ProductId, model.StockQuantity, ct);

        if (productUpdated && stockUpdated)
        {
            TempData["ToastMessage"] = "Товар обновлен.";
            TempData["ToastType"] = "success";
        }
        else if (productUpdated)
        {
            TempData["ToastMessage"] = "Товар обновлен, но остаток не обновлен.";
            TempData["ToastType"] = "warning";
        }
        else
        {
            TempData["ToastMessage"] = "Не удалось обновить товар.";
            TempData["ToastType"] = "error";
        }
        return RedirectToAction("Index", new { tab = "products" });
    }

    [HttpPost("/admin/product/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(Guid productId, CancellationToken ct)
    {
        if (productId == Guid.Empty)
        {
            TempData["ToastMessage"] = "Укажите товар.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", new { tab = "products" });
        }

        var success = await _apiService.DeleteProductAsync(productId, ct);
        TempData["ToastMessage"] = success ? "Товар удален." : "Не удалось удалить товар.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "products" });
    }

    [HttpPost("/admin/product/import-json")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportProductsJson(IFormFile? jsonFile, CancellationToken ct)
    {
        var items = await ParseJsonArrayFromUploadAsync<AdminBulkProductImportItemViewModel>(jsonFile, ct, "товаров");
        if (items is null)
        {
            return RedirectToAction("Index", new { tab = "products" });
        }

        var success = await _apiService.BulkImportProductsAsync(items, ct);
        TempData["ToastMessage"] = success
            ? $"Импорт товаров завершен. Обработано: {items.Count}."
            : "Не удалось импортировать товары.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "products" });
    }

    [HttpGet("/admin/templates/products.json")]
    public IActionResult DownloadProductsTemplate()
    {
        var template = new[]
        {
            new
            {
                name = "Фильтр Aqua X1",
                description = "Полное описание товара",
                shortDescription = "Краткое описание",
                price = 199.99m,
                oldPrice = 249.99m,
                filterType = 0,
                categoryName = "Проточные фильтры",
                brandName = "AquaPro",
                stockQuantity = 25,
                sku = "AQX1-001",
                filterLifespanMonths = 12,
                filterCapacityLiters = 4000,
                flowRateLitersPerMinute = 2.5,
                imageUrls = new[]
                {
                    "https://example.com/images/filter-x1-main.jpg",
                    "https://example.com/images/filter-x1-side.jpg"
                }
            }
        };

        return BuildJsonTemplateFile(template, "products-import-template.json");
    }

    [HttpGet("/admin/templates/categories.json")]
    public IActionResult DownloadCategoriesTemplate()
    {
        var template = new[]
        {
            new
            {
                name = "Проточные фильтры",
                description = "Категория для фильтров проточного типа",
                imageUrl = "https://example.com/images/categories/flow-filters.jpg"
            }
        };

        return BuildJsonTemplateFile(template, "categories-import-template.json");
    }

    [HttpGet("/admin/templates/brands.json")]
    public IActionResult DownloadBrandsTemplate()
    {
        var template = new[]
        {
            new
            {
                name = "AquaPro",
                description = "Производитель систем очистки воды",
                country = "Беларусь",
                logoUrl = "https://example.com/images/brands/aquapro-logo.png",
                websiteUrl = "https://aquapro.example"
            }
        };

        return BuildJsonTemplateFile(template, "brands-import-template.json");
    }

    [HttpPost("/admin/order/status/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, int status, int ordersPage, CancellationToken ct)
    {
        if (orderId == Guid.Empty)
        {
            TempData["ToastMessage"] = "Укажите заказ.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", new { tab = "orders", ordersPage = ordersPage < 1 ? 1 : ordersPage });
        }

        var success = await _apiService.UpdateOrderStatusAsync(orderId, status, ct);
        TempData["ToastMessage"] = success ? "Статус заказа обновлён." : "Не удалось обновить статус заказа.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "orders", ordersPage = ordersPage < 1 ? 1 : ordersPage });
    }

    // ───── Users (admin) ─────

    [HttpPost("/admin/user/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUser(AdminUserViewModel model, CancellationToken ct)
    {
        var success = await _apiService.UpdateUserAsync(model, ct);
        TempData["ToastMessage"] = success ? "Пользователь обновлён." : "Не удалось обновить пользователя.";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToAction("Index", new { tab = "users" });
    }

    // ───── Orders export ─────

    [HttpGet("/admin/orders/export/csv")]
    public async Task<IActionResult> ExportOrdersCsv(CancellationToken ct)
    {
        var (content, fileName, contentType) = await _apiService.ExportOrdersCsvAsync(ct);

        if (content is null || content.Length == 0)
        {
            TempData["ToastMessage"] = "Не удалось получить файл статистики заказов.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", new { tab = "orders" });
        }

        return File(
            content,
            string.IsNullOrWhiteSpace(contentType) ? "text/csv" : contentType,
            string.IsNullOrWhiteSpace(fileName) ? $"orders-stat-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv" : fileName);
    }

    [HttpGet("/admin/analytics/export/csv")]
    public async Task<IActionResult> ExportAnalyticsCsv(CancellationToken ct)
    {
        var (content, fileName, contentType) = await _apiService.ExportOrderAnalyticsCsvAsync(ct);

        if (content is null || content.Length == 0)
        {
            TempData["ToastMessage"] = "Не удалось получить файл расширенной аналитики.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", new { tab = "analytics" });
        }

        return File(
            content,
            string.IsNullOrWhiteSpace(contentType) ? "text/csv" : contentType,
            string.IsNullOrWhiteSpace(fileName) ? $"analytics-stat-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv" : fileName);
    }

    private async Task<List<T>?> ParseJsonArrayFromUploadAsync<T>(IFormFile? jsonFile, CancellationToken ct, string entityName)
    {
        if (jsonFile is null || jsonFile.Length == 0)
        {
            TempData["ToastMessage"] = "Выберите JSON-файл для импорта.";
            TempData["ToastType"] = "error";
            return null;
        }

        if (!string.Equals(Path.GetExtension(jsonFile.FileName), ".json", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ToastMessage"] = "Поддерживаются только файлы .json";
            TempData["ToastType"] = "error";
            return null;
        }

        try
        {
            await using var stream = jsonFile.OpenReadStream();
            var items = await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions, ct);

            if (items is null || items.Count == 0)
            {
                TempData["ToastMessage"] = $"JSON-файл {entityName} пуст или имеет неверный формат.";
                TempData["ToastType"] = "error";
                return null;
            }

            return items;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Некорректный JSON при импорте {EntityName}", entityName);
            TempData["ToastMessage"] = $"Некорректный JSON для импорта {entityName}.";
            TempData["ToastType"] = "error";
            return null;
        }
    }

    private static FileContentResult BuildJsonTemplateFile<T>(T template, string fileName)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(template, JsonTemplateOptions);
        return new FileContentResult(bytes, "application/json")
        {
            FileDownloadName = fileName
        };
    }
}
