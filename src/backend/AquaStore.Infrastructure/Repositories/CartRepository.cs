using AquaStore.Domain.Cart;
using AquaStore.Infrastructure.Data;
using Common.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AquaStore.Infrastructure.Repositories;

public class CartRepository : Repository<Cart>, ICartRepository
{
    private readonly AquaStoreDbContext _dbContext;

    public CartRepository(AquaStoreDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<Cart?> GetWithItemsAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
    }

    public async Task<Cart> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetByUserIdAsync(userId, cancellationToken);
        
        if (cart is null)
        {
            cart = Cart.Create(userId);
            Add(cart);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return cart;
    }
}
