using AquaStore.Domain.Brands;
using AquaStore.Domain.Cart;
using AquaStore.Domain.Categories;
using AquaStore.Domain.Orders;
using AquaStore.Domain.Products;
using AquaStore.Domain.Reviews;
using AquaStore.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace AquaStore.Infrastructure.Data;

public class AquaStoreDbContext : DbContext
{
    public AquaStoreDbContext(DbContextOptions<AquaStoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AquaStoreDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}

