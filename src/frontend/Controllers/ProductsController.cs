using Microsoft.AspNetCore.Mvc;
using frontend.Services;
using frontend.ViewModels;
using System.Linq;

namespace frontend.Controllers;

public class ProductsController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IApiService apiService, ILogger<ProductsController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [Route("/products/{slug}")]
    [HttpGet]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _apiService.GetProductBySlugAsync(slug, cancellationToken);
            if (product == null)
            {
                return NotFound();
            }

            var reviewsPage = Request.Query.TryGetValue("reviewsPage", out var pageValues)
                && int.TryParse(pageValues.ToString(), out var parsedPage)
                ? Math.Max(parsedPage, 1)
                : 1;
            var reviewsPageSize = 5;

            var reviews = await _apiService.GetReviewsByProductIdAsync(
                product.Id,
                reviewsPage,
                reviewsPageSize,
                cancellationToken);
            var similarProducts = await GetSimilarProductsAsync(product, cancellationToken);

            var model = new ProductDetailsViewModel
            {
                Product = product,
                SimilarProducts = similarProducts,
                Reviews = reviews,
                NewReview = new ReviewCreateViewModel
                {
                    ProductId = product.Id,
                    Slug = product.Slug
                },
                ReviewsPageSize = reviewsPageSize
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке товара по slug: {Slug}", slug);
            return NotFound();
        }
    }

    [Route("/products/{slug}/reviews")]
    [HttpGet]
    public async Task<IActionResult> GetReviews(
        string slug,
        int pageNumber = 1,
        int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        var product = await _apiService.GetProductBySlugAsync(slug, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        var reviews = await _apiService.GetReviewsByProductIdAsync(
            product.Id,
            pageNumber,
            pageSize,
            cancellationToken);

        return Json(new { success = true, data = reviews });
    }

    [Route("/products/{slug}/reviews")]
    [HttpPost]
    public async Task<IActionResult> CreateReview(
        string slug,
        [Bind(Prefix = "NewReview")] ReviewCreateViewModel model,
        CancellationToken cancellationToken)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            if (Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return Unauthorized(new { success = false, message = "Требуется вход" });
            }
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            var product = await _apiService.GetProductBySlugAsync(slug, cancellationToken);
            if (product == null)
            {
                return NotFound();
            }

            var reviews = await _apiService.GetReviewsByProductIdAsync(product.Id, 1, 5, cancellationToken);
            var similarProducts = await GetSimilarProductsAsync(product, cancellationToken);
            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                SimilarProducts = similarProducts,
                Reviews = reviews,
                NewReview = model
            };
            if (Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return BadRequest(new { success = false, message = "Некорректные данные" });
            }
            return View("Details", viewModel);
        }

        var result = await _apiService.CreateReviewAsync(model.ProductId, model.Rating, model.Comment, cancellationToken);
        if (!result.Success)
        {
            if (Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return BadRequest(new { success = false, message = result.Message ?? "Не удалось отправить отзыв." });
            }
            ModelState.AddModelError(string.Empty, result.Message ?? "Не удалось отправить отзыв.");
        }

        if (Request.Headers.Accept.ToString().Contains("application/json"))
        {
            return Ok(new { success = true });
        }

        return RedirectToAction(nameof(Details), new { slug, reviewsPage = 1 });
    }

    private async Task<List<ProductViewModel>> GetSimilarProductsAsync(
        ProductViewModel product,
        CancellationToken cancellationToken)
    {
        const int desiredCount = 4;
        const int pageSize = 16;

        PagedResponse<ProductViewModel>? pagedProducts = null;

        if (product.CategoryId != Guid.Empty)
        {
            pagedProducts = await _apiService.GetProductsAsync(
                pageNumber: 1,
                pageSize: pageSize,
                categoryId: product.CategoryId,
                cancellationToken: cancellationToken);
        }

        if ((pagedProducts?.Items?.Count ?? 0) == 0 && product.BrandId != Guid.Empty)
        {
            pagedProducts = await _apiService.GetProductsAsync(
                pageNumber: 1,
                pageSize: pageSize,
                brandId: product.BrandId,
                cancellationToken: cancellationToken);
        }

        var candidates = pagedProducts?.Items ?? new List<ProductViewModel>();

        return candidates
            .Where(p => p.Id != product.Id)
            .OrderByDescending(p => string.Equals(p.BrandName, product.BrandName, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.AverageRating ?? 0)
            .Take(desiredCount)
            .ToList();
    }
}

