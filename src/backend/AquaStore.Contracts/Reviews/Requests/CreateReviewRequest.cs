namespace AquaStore.Contracts.Reviews.Requests;

/// <summary>
/// Запрос на создание отзыва
/// </summary>
public sealed record CreateReviewRequest(
    int Rating,
    string? Comment);

