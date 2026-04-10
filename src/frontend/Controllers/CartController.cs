using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using frontend.Services;
using frontend.ViewModels;

namespace frontend.Controllers;

public class CartController : Controller
{
    private static readonly string[] AddressModelStateKeys =
    {
        "Order.City",
        "Order.Street",
        "Order.Building",
        "Order.Apartment",
        "Order.PostalCode"
    };

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
            if (result.Success)
            {
                return Ok(new { success = true, message = "Товар добавлен в корзину" });
            }

            var message = ResolveStockAwareMessage(result, "Не удалось добавить товар в корзину");
            return BadRequest(new { success = false, message });
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

            var profile = await _apiService.GetProfileAsync();
            model.SavedAddresses = profile?.Addresses
                ?.OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Id)
                .ToList()
                ?? new List<UserAddressViewModel>();

            if (model.SavedAddresses.Count > 0)
            {
                var selectedAddress = model.SavedAddresses.First();
                model.SelectedAddressId = selectedAddress.Id;
                model.UseManualAddress = false;
                ApplyAddressToOrder(model.Order, selectedAddress);
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

            var profile = await _apiService.GetProfileAsync(cancellationToken);
            model.SavedAddresses = profile?.Addresses
                ?.OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Id)
                .ToList()
                ?? new List<UserAddressViewModel>();

            if (!model.UseManualAddress)
            {
                foreach (var key in AddressModelStateKeys)
                {
                    ModelState.Remove(key);
                }

                if (model.SelectedAddressId is null)
                {
                    ModelState.AddModelError(nameof(model.SelectedAddressId), "Выберите сохраненный адрес или введите адрес вручную.");
                }
                else
                {
                    var selectedAddress = model.SavedAddresses.FirstOrDefault(a => a.Id == model.SelectedAddressId.Value);
                    if (selectedAddress is null)
                    {
                        ModelState.AddModelError(nameof(model.SelectedAddressId), "Выбранный адрес не найден.");
                    }
                    else
                    {
                        ApplyAddressToOrder(model.Order, selectedAddress);
                    }
                }

                TryValidateModel(model.Order, nameof(model.Order));
            }

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

            if (model.UseManualAddress && model.SaveAddressToProfile)
            {
                var addAddressResult = await _apiService.AddAddressAsync(new AddAddressViewModel
                {
                    City = model.Order.City,
                    Street = model.Order.Street,
                    Building = model.Order.Building,
                    Apartment = model.Order.Apartment,
                    PostalCode = model.Order.PostalCode,
                    IsDefault = model.SavedAddresses.Count == 0
                }, cancellationToken);

                if (!addAddressResult.Success)
                {
                    ModelState.AddModelError(string.Empty, addAddressResult.Message ?? "Не удалось сохранить адрес в профиле.");
                    return View(model);
                }
            }

            var result = await _apiService.CreateOrderAsync(model.Order, cancellationToken);
            if (!result.Success || result.Data is null)
            {
                var message = ResolveStockAwareMessage(result, "Не удалось оформить заказ.");
                ModelState.AddModelError(string.Empty, message);
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

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(Guid productId, int quantity)
    {
        try
        {
            if (quantity <= 0)
            {
                return BadRequest(new { success = false, message = "Количество должно быть больше нуля." });
            }

            var result = await _apiService.UpdateCartItemQuantityAsync(productId, quantity);
            if (result.Success)
            {
                return Ok(new { success = true, message = "Количество обновлено." });
            }

            var message = ResolveStockAwareMessage(result, "Не удалось обновить количество товара.");
            return BadRequest(new { success = false, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении количества товара в корзине: {ProductId}", productId);
            return StatusCode(500, new { success = false, message = "Произошла ошибка" });
        }
    }

    private static string ResolveStockAwareMessage(ApiOperationResult result, string fallback)
    {
        if (result.Errors?.Any(e => e.Code == "Product.InsufficientStock") == true)
        {
            return "Товара нет в наличии или недостаточно на складе.";
        }

        return result.Message ?? fallback;
    }

    private static void ApplyAddressToOrder(CreateOrderViewModel order, UserAddressViewModel address)
    {
        order.City = address.City;
        order.Street = address.Street;
        order.Building = address.Building;
        order.Apartment = address.Apartment;
        order.PostalCode = address.PostalCode;
    }

    private static string ResolveStockAwareMessage<T>(ApiOperationResult<T> result, string fallback)
    {
        if (result.Errors?.Any(e => e.Code == "Product.InsufficientStock") == true)
        {
            return "Товара нет в наличии или недостаточно на складе.";
        }

        return result.Message ?? fallback;
    }
}

