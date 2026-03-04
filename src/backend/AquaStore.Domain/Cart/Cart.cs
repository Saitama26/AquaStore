using Common.Domain.Primitives;
using AquaStore.Domain.Users;
using AquaStore.Domain.ValueObjects;

namespace AquaStore.Domain.Cart;

/// <summary>
/// Корзина покупателя
/// </summary>
public sealed class Cart : AggregateRoot, IAuditableEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<CartItem> _items = [];
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart() { }

    public static Cart Create(Guid userId)
    {
        return new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void AddItem(Guid productId, Money price, int quantity = 1)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = CartItem.Create(Id, productId, price, quantity);
            _items.Add(item);
        }
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);

        if (item is null)
            throw new InvalidOperationException("Item not found in cart");

        if (quantity <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.UpdateQuantity(quantity);
        }
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);

        if (item is not null)
        {
            _items.Remove(item);
        }
    }

    public void Clear()
    {
        _items.Clear();
    }

    public int TotalItems => _items.Sum(i => i.Quantity);

    public Money TotalAmount
    {
        get
        {
            if (_items.Count == 0)
                return Money.Zero();

            return _items
                .Select(i => i.TotalPrice)
                .Aggregate((a, b) => a.Add(b));
        }
    }

    public bool IsEmpty => _items.Count == 0;
}

