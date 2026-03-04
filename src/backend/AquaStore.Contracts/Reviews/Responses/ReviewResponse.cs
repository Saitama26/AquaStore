namespace AquaStore.Contracts.Reviews.Responses;

/// <summary>
/// Информация об отзыве
/// </summary>
public sealed record ReviewResponse(
    Guid Id,
    Guid ProductId,
    Guid UserId,
    string UserName,
    int Rating,
    string? Comment,
    bool IsApproved,
    DateTime CreatedAt);

