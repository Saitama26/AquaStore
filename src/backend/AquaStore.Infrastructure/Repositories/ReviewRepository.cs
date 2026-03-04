using AquaStore.Domain.Reviews;
using AquaStore.Infrastructure.Data;
using Common.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AquaStore.Infrastructure.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(AquaStoreDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Review>> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Review> Items, int TotalCount)> GetPagedByProductIdAsync(
        Guid productId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Review>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Product)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserHasReviewedProductAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            r => r.UserId == userId && r.ProductId == productId,
            cancellationToken);
    }

    public async Task<double?> GetAverageRatingAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var reviews = await DbSet
            .Where(r => r.ProductId == productId && r.IsApproved)
            .ToListAsync(cancellationToken);

        if (!reviews.Any())
            return null;

        return reviews.Average(r => r.Rating.Value);
    }
}
