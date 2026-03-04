namespace Common.Application.Models;

/// <summary>
/// Параметры запроса с пагинацией
/// </summary>
public record PagedRequest
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// Номер страницы (начинается с 1)
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Размер страницы
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? DefaultPageSize : value;
    }

    /// <summary>
    /// Поле для сортировки
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Сортировка по убыванию
    /// </summary>
    public bool SortDescending { get; set; }

    /// <summary>
    /// Количество элементов для пропуска
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;
}

