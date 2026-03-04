using frontend.Services;
using frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace frontend.ViewComponents;

public class NavigationMenuViewComponent : ViewComponent
{
    private readonly IApiService _apiService;
    private readonly ILogger<NavigationMenuViewComponent> _logger;

    public NavigationMenuViewComponent(IApiService apiService, ILogger<NavigationMenuViewComponent> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            var categoriesTask = _apiService.GetCategoriesAsync();
            var brandsTask = _apiService.GetBrandsAsync();

            await Task.WhenAll(categoriesTask, brandsTask);

            var categories = (await categoriesTask) ?? Array.Empty<CategoryViewModel>();
            var brands = (await brandsTask) ?? Array.Empty<BrandViewModel>();

            var model = new NavigationMenuViewModel
            {
                Categories = categories.Where(c => c.IsActive).ToList(),
                Brands = brands.Where(b => b.IsActive).ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке меню навигации");
            return View(new NavigationMenuViewModel());
        }
    }
}

