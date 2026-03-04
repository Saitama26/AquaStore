using Common.Domain.Primitives;
using AquaStore.Domain.Products;
using AquaStore.Domain.ValueObjects;

namespace AquaStore.Domain.Cart;

/// <summary>
/// Элемент корзины
/// </summary>
public sealed class CartItem : Entity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }

    private CartItem() { }

    internal static CartItem Create(Guid cartId, Guid productId, Money unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        return new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            ProductId = productId,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Quantity = quantity;
    }

    public void UpdatePrice(Money price)
    {
        UnitPrice = price;
    }

    public Money TotalPrice => UnitPrice.Multiply(Quantity);
}

