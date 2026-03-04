using Microsoft.AspNetCore.Mvc;

namespace AquaStore.Api.Controllers;

/// <summary>
/// Контроллер для проверки здоровья приложения
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Проверка здоровья API
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}

