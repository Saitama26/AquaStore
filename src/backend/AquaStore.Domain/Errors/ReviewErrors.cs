using Common.Domain.Errors;

namespace AquaStore.Domain.Errors;

/// <summary>
/// Ошибки, связанные с отзывами
/// </summary>
public static class ReviewErrors
{
    public static Error NotFound(Guid reviewId) =>
        Error.NotFound("Review.NotFound", $"Review with ID '{reviewId}' was not found");

    public static Error AlreadyReviewed(Guid productId) =>
        Error.Conflict("Review.AlreadyExists", $"User has already reviewed product '{productId}'");

    public static Error InvalidRating =>
        Error.Validation("Review.InvalidRating", "Rating must be between 1 and 5");

    public static Error CannotReviewOwnProduct =>
        Error.Validation("Review.CannotReviewOwn", "Cannot review your own product");

    public static Error MustPurchaseToReview =>
        Error.Validation("Review.MustPurchase", "You must purchase the product before leaving a review");
}

