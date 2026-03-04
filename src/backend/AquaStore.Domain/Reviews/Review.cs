using Common.Domain.Primitives;
using AquaStore.Domain.ValueObjects;
using AquaStore.Domain.Users;
using AquaStore.Domain.Products;

namespace AquaStore.Domain.Reviews;

/// <summary>
/// Отзыв о товаре
/// </summary>
public sealed class Review : AggregateRoot, IAuditableEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Rating Rating { get; private set; } = null!;
    public string? Comment { get; private set; }
    public bool IsApproved { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private Review() { }

    public static Review Create(
        Guid productId,
        Guid userId,
        int rating,
        string? comment = null)
    {
        return new Review
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UserId = userId,
            Rating = Rating.Create(rating),
            Comment = comment,
            IsApproved = false,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(int rating, string? comment)
    {
        Rating = Rating.Create(rating);
        Comment = comment;
    }

    public void Approve() => IsApproved = true;

    public void Reject() => IsApproved = false;
}

