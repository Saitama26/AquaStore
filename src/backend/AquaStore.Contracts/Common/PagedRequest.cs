namespace AquaStore.Contracts.Common;

/// <summary>
/// Запрос с пагинацией
/// </summary>
public record PagedRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}

