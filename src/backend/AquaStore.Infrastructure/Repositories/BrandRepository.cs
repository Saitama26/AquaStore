using AquaStore.Domain.Brands;
using AquaStore.Infrastructure.Data;
using Common.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AquaStore.Infrastructure.Repositories;

public class BrandRepository : Repository<Brand>, IBrandRepository
{
    public BrandRepository(AquaStoreDbContext context) : base(context)
    {
    }

    public async Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(b => b.Slug.Value == slug, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(b => b.Slug.Value == slug);
        
        if (excludeId.HasValue)
            query = query.Where(b => b.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
