using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using frontend.Services;
using frontend.ViewModels;

namespace frontend.Controllers;

public class CartController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<CartController> _logger;

    public CartController(IApiService apiService, ILogger<CartController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid productId, int quantity = 1)
    {
        try
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new { success = false, message = "Требуется вход в аккаунт" });
            }

            var result = await _apiService.AddToCartAsync(productId, quantity);
            if (result)
            {
                return Ok(new { success = true, message = "Товар добавлен в корзину" });
            }
            return BadRequest(new { success = false, message = "Не удалось добавить товар в корзину" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении товара в корзину: {ProductId}", productId);
            return StatusCode(500, new { success = false, message = "Произошла ошибка" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCount()
    {
        try
        {
            var cart = await _apiService.GetCartAsync();
            return Content(cart?.TotalItems.ToString() ?? "0");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении количества товаров в корзине");
            return Content("0");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var cart = await _apiService.GetCartAsync();
            return View(cart ?? new ViewModels.CartViewModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке корзины");
            return View(new ViewModels.CartViewModel());
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Checkout([FromQuery] List<Guid>? productIds, [FromQuery] bool buyNow = false)
    {
        try
        {
            var cart = await _apiService.GetCartAsync();
            if (cart is null || cart.Items.Count == 0)
            {
                TempData["ToastMessage"] = "Корзина пуста.";
                TempData["ToastType"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            var model = new CheckoutViewModel
            {
                Cart = cart
            };

            var selectedIds = productIds?
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToHashSet();

            if (selectedIds is { Count: > 0 })
            {
                model.Cart.Items = model.Cart.Items
                    .Where(i => selectedIds.Contains(i.ProductId))
                    .ToList();
                model.Cart.TotalItems = model.Cart.Items.Sum(i => i.Quantity);
                model.Cart.TotalAmount = model.Cart.Items.Sum(i => i.TotalPrice);
                model.Order.SelectedProductIds = model.Cart.Items
                    .Select(i => i.ProductId)
                    .ToList();
            }
            else
            {
                model.Order.SelectedProductIds = model.Cart.Items
                    .Select(i => i.ProductId)
                    .ToList();
            }

            model.Order.BuyNowSingleUnit = buyNow && model.Order.SelectedProductIds.Count == 1;

            if (model.Order.SelectedProductIds.Count == 0)
            {
                TempData["ToastMessage"] = "Выберите хотя бы один товар для оформления.";
                TempData["ToastType"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при переходе к оформлению заказа");
            TempData["ToastMessage"] = "Не удалось открыть оформление заказа.";
            TempData["ToastType"] = "error";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var cart = await _apiService.GetCartAsync(cancellationToken);
            model.Cart = cart ?? new CartViewModel();

            model.Order.SelectedProductIds = model.Order.SelectedProductIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (model.Order.SelectedProductIds.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Выберите хотя бы один товар для оформления.");
            }

            model.Cart.Items = model.Cart.Items
                .Where(i => model.Order.SelectedProductIds.Contains(i.ProductId))
                .ToList();
            model.Cart.TotalItems = model.Cart.Items.Sum(i => i.Quantity);
            model.Cart.TotalAmount = model.Cart.Items.Sum(i => i.TotalPrice);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (cart is null || cart.Items.Count == 0)
            {
                TempData["ToastMessage"] = "Корзина пуста.";
                TempData["ToastType"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            var result = await _apiService.CreateOrderAsync(model.Order, cancellationToken);
            if (!result.Success || result.Data is null)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Не удалось оформить заказ.");
                return View(model);
            }

            return RedirectToAction(nameof(Receipt), new { id = result.Data.OrderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при оформлении заказа");
            ModelState.AddModelError(string.Empty, "Произошла ошибка при оформлении заказа.");
            return View(model);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Receipt(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _apiService.GetOrderByIdAsync(id, cancellationToken);
            if (order is null)
            {
                return NotFound();
            }

            return View(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке квитанции: {OrderId}", id);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Remove(Guid productId)
    {
        try
        {
            var result = await _apiService.RemoveFromCartAsync(productId);
            if (result)
            {
                return Ok(new { success = true, message = "Товар удален из корзины" });
            }
            return BadRequest(new { success = false, message = "Не удалось удалить товар из корзины" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении товара из корзины: {ProductId}", productId);
            return StatusCode(500, new { success = false, message = "Произошла ошибка" });
        }
    }
}

