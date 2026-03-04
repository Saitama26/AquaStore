namespace Common.Application.Models;

/// <summary>
/// Результат запроса с пагинацией
/// </summary>
/// <typeparam name="T">Тип элементов</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>
    /// Элементы текущей страницы
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Общее количество элементов
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Номер текущей страницы
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Размер страницы
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Общее количество страниц
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Есть ли предыдущая страница
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Есть ли следующая страница
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Создать пустой результат
    /// </summary>
    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10) =>
        new([], 0, pageNumber, pageSize);

    /// <summary>
    /// Преобразовать элементы
    /// </summary>
    public PagedResult<TDestination> Map<TDestination>(Func<T, TDestination> mapper)
    {
        var mappedItems = Items.Select(mapper).ToList();
        return new PagedResult<TDestination>(mappedItems, TotalCount, PageNumber, PageSize);
    }
}

