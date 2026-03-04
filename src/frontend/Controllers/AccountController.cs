using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using frontend.Services;
using frontend.ViewModels;

namespace frontend.Controllers;

public class AccountController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IApiService apiService, ILogger<AccountController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [Route("/account/register")]
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["HideHeaderSearch"] = true;
        ViewData["HideBurger"] = true;
        ViewData["HideAuthButtons"] = true;
        return View(new RegisterViewModel());
    }

    [Route("/account/register")]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        ViewData["HideHeaderSearch"] = true;
        ViewData["HideBurger"] = true;
        ViewData["HideAuthButtons"] = true;
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        try
        {
            var result = await _apiService.RegisterAsync(
                model.Email,
                model.Password,
                model.ConfirmPassword,
                model.FirstName,
                model.LastName,
                model.Phone,
                cancellationToken);

            if (result.Success)
            {
                return RedirectToAction(nameof(ConfirmRegistration), new { email = model.Email });
            }

            AddApiErrorsToModelState(result, isConfirmFlow: false);
            return View("Index", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации");
            ModelState.AddModelError(string.Empty, "Ошибка сервера при регистрации.");
            return View("Index", model);
        }
    }

    [Route("/account/register/confirm")]
    [HttpGet]
    public IActionResult ConfirmRegistration(string? email)
    {
        ViewData["HideHeaderSearch"] = true;
        ViewData["HideBurger"] = true;
        ViewData["HideAuthButtons"] = true;
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction("Index");
        }

        return View(new ConfirmRegistrationViewModel { Email = email });
    }

    [Route("/account/register/confirm")]
    [HttpPost]
    public async Task<IActionResult> ConfirmRegistration(ConfirmRegistrationViewModel model, CancellationToken cancellationToken)
    {
        ViewData["HideHeaderSearch"] = true;
        ViewData["HideBurger"] = true;
        ViewData["HideAuthButtons"] = true;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _apiService.ConfirmRegistrationAsync(
                model.Email,
                model.Code,
                cancellationToken);

            if (result.Success)
            {
                TempData["ToastMessage"] = "Регистрация подтверждена. Войдите в аккаунт.";
                TempData["ToastType"] = "success";
                return RedirectToAction("Login");
            }

            AddApiErrorsToModelState(result, isConfirmFlow: true);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при подтверждении регистрации");
            ModelState.AddModelError(string.Empty, "Ошибка сервера при подтверждении регистрации.");
            return View(model);
        }
    }

    private void AddApiErrorsToModelState(ApiOperationResult result, bool isConfirmFlow)
    {
        if (result.Errors is { Count: > 0 })
        {
            foreach (var error in result.Errors)
            {
                var target = error.Code switch
                {
                    nameof(RegisterViewModel.FirstName) => nameof(RegisterViewModel.FirstName),
                    nameof(RegisterViewModel.LastName) => nameof(RegisterViewModel.LastName),
                    nameof(RegisterViewModel.Password) => nameof(RegisterViewModel.Password),
                    nameof(RegisterViewModel.ConfirmPassword) => nameof(RegisterViewModel.ConfirmPassword),
                    nameof(RegisterViewModel.Phone) => nameof(RegisterViewModel.Phone),
                    nameof(ConfirmRegistrationViewModel.Code) => nameof(ConfirmRegistrationViewModel.Code),
                    nameof(RegisterViewModel.Email) => isConfirmFlow
                        ? nameof(ConfirmRegistrationViewModel.Email)
                        : nameof(RegisterViewModel.Email),
                    _ => string.Empty
                };

                ModelState.AddModelError(target, error.Message);
            }

            return;
        }

        if (!string.IsNullOrWhiteSpace(result.Message))
        {
            ModelState.AddModelError(string.Empty, result.Message);
        }
    }

    [Route("/account/login")]
    [HttpGet]
    public IActionResult Login()
    {
        ViewData["HideHeaderSearch"] = true;
        ViewData["HideBurger"] = true;
        ViewData["HideAuthButtons"] = true;
        return View(new LoginViewModel());
    }

    [Route("/account/login")]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        ViewData["HideHeaderSearch"] = true;
        ViewData["HideBurger"] = true;
        ViewData["HideAuthButtons"] = true;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var authResponse = await _apiService.LoginAsync(model.Email, model.Password, cancellationToken);
            if (authResponse != null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, authResponse.UserId.ToString()),
                    new(ClaimTypes.Email, authResponse.Email),
                    new(ClaimTypes.Name, $"{authResponse.FirstName} {authResponse.LastName}".Trim()),
                    new(ClaimTypes.Role, authResponse.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = authResponse.ExpiresAt
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                Response.Cookies.Append(
                    "access_token",
                    authResponse.AccessToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = authResponse.ExpiresAt
                    });

                TempData["ToastMessage"] = "Вход выполнен.";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при входе");
            ModelState.AddModelError(string.Empty, "Ошибка сервера при входе.");
            return View(model);
        }
    }

    [Route("/account/logout")]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete("access_token");
        return RedirectToAction("Index", "Home");
    }

    [Route("/account/profile")]
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        try
        {
            var profileTask = _apiService.GetProfileAsync(cancellationToken);
            var ordersTask = _apiService.GetOrdersAsync(1, 50, cancellationToken);

            await Task.WhenAll(profileTask, ordersTask);

            var profile = await profileTask;
            var orders = await ordersTask;

            var model = new ProfileViewModel
            {
                Orders = orders,
                Email = profile?.Email ?? User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "",
                FirstName = profile?.FirstName ?? "",
                LastName = profile?.LastName ?? "",
                Phone = profile?.Phone,
                Role = profile?.Role ?? "",
                CreatedAt = profile?.CreatedAt ?? DateTime.MinValue
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке профиля");
            return View(new ProfileViewModel());
        }
    }

    [Route("/account/profile/update")]
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(
        string firstName,
        string lastName,
        string? phone,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _apiService.UpdateProfileAsync(firstName, lastName, phone, cancellationToken);
            if (result.Success)
            {
                // Update the auth cookie claims with the new name
                var currentPrincipal = HttpContext.User;
                var claims = new List<System.Security.Claims.Claim>();
                foreach (var c in currentPrincipal.Claims)
                {
                    if (c.Type == System.Security.Claims.ClaimTypes.Name)
                        claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, $"{firstName} {lastName}".Trim()));
                    else
                        claims.Add(c);
                }

                var claimsIdentity = new System.Security.Claims.ClaimsIdentity(
                    claims,
                    Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(
                    Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                    new System.Security.Claims.ClaimsPrincipal(claimsIdentity),
                    authProperties);

                TempData["ToastMessage"] = "Профиль обновлён.";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = result.Message ?? "Ошибка при обновлении профиля.";
                TempData["ToastType"] = "error";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении профиля");
            TempData["ToastMessage"] = "Ошибка сервера.";
            TempData["ToastType"] = "error";
        }

        return RedirectToAction(nameof(Profile));
    }
}