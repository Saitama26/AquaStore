using frontend.Services;
using frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace frontend.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
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

        await Task.WhenAll(productsTask, categoriesTask, brandsTask, ordersTask, usersTask);

        model.Products = await productsTask ?? new PagedResponse<ProductViewModel>();
        model.Categories = (await categoriesTask) ?? Array.Empty<CategoryViewModel>();
        model.Brands = (await brandsTask) ?? Array.Empty<BrandViewModel>();
        model.Orders = await ordersTask;
        var usersResponse = await usersTask;
        model.Users = usersResponse?.Items ?? new List<AdminUserViewModel>();

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

        var success = await _apiService.UpdateProductAsync(model, ct);
        TempData["ToastMessage"] = success ? "Товар обновлен." : "Не удалось обновить товар.";
        TempData["ToastType"] = success ? "success" : "error";
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
}
