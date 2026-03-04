using Microsoft.AspNetCore.Mvc;
using frontend.Services;
using frontend.ViewModels;

namespace frontend.Controllers;

public class HomeController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IApiService apiService, ILogger<HomeController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [Route("/")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var categories = await _apiService.GetCategoriesAsync() ?? Array.Empty<CategoryViewModel>();
            var brands = await _apiService.GetBrandsAsync() ?? Array.Empty<BrandViewModel>();

            // Featured products (up to 8)
            var featuredPage = await _apiService.GetProductsAsync(
                pageNumber: 1, pageSize: 8,
                sortBy: "created", sortDescending: true);

            // New products (up to 4)
            var newPage = await _apiService.GetProductsAsync(
                pageNumber: 1, pageSize: 4,
                sortBy: "created", sortDescending: true);

            var model = new LandingViewModel
            {
                Categories = categories,
                Brands = brands,
                FeaturedProducts = featuredPage?.Items ?? new List<ProductViewModel>(),
                NewProducts = newPage?.Items ?? new List<ProductViewModel>(),
                TotalProductsCount = featuredPage?.TotalCount ?? 0,
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке главной страницы");
            return View(new LandingViewModel());
        }
    }

    [Route("/catalog")]
    public async Task<IActionResult> Catalog(
        [FromQuery] int pageNumber = 1,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? brandId = null,
        [FromQuery] int? filterType = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? inStock = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            var pageSize = 12;
            var paged = await _apiService.GetProductsAsync(
                pageNumber, pageSize, searchTerm, categoryId, brandId,
                filterType, minPrice, maxPrice, inStock, sortBy, sortDescending);

            var categories = await _apiService.GetCategoriesAsync();
            var brands = await _apiService.GetBrandsAsync();

            ViewData["CategoryId"] = categoryId;
            ViewData["BrandId"] = brandId;
            ViewData["SearchTerm"] = searchTerm;
            ViewData["FilterType"] = filterType;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["InStock"] = inStock;
            ViewData["SortBy"] = sortBy;
            ViewData["SortDescending"] = sortDescending;
            ViewData["Categories"] = categories;
            ViewData["Brands"] = brands;

            return View(paged ?? new PagedResponse<ProductViewModel> { Items = new List<ProductViewModel>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке товаров");
            return View(new PagedResponse<ProductViewModel> { Items = new List<ProductViewModel>() });
        }
    }

    [HttpGet("/home/load-more")]
    public async Task<IActionResult> LoadMore(
        [FromQuery] int pageNumber,
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? brandId,
        [FromQuery] int? filterType = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? inStock = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            var pageSize = 12;
            var paged = await _apiService.GetProductsAsync(
                pageNumber, pageSize, searchTerm, categoryId, brandId,
                filterType, minPrice, maxPrice, inStock, sortBy, sortDescending);
            var items = paged?.Items ?? new List<ProductViewModel>();
            return PartialView("_ProductCards", items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при подгрузке товаров");
            return PartialView("_ProductCards", new List<ProductViewModel>());
        }
    }
}
