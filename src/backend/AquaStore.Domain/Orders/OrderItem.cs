using Common.Domain.Primitives;
using AquaStore.Domain.Products;
using AquaStore.Domain.ValueObjects;

namespace AquaStore.Domain.Orders;

/// <summary>
/// Позиция заказа
/// </summary>
public sealed class OrderItem : Entity
{
    public Guid OrderId { get; private set; }

    public Guid? ProductId { get; private set; }
    public Product? Product { get; private set; }

    public string ProductName { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }

    private OrderItem() { }

    internal static OrderItem Create(
        Guid orderId,
        Guid? productId,
        string productName,
        Money unitPrice,
        int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }

    public Money TotalPrice => UnitPrice.Multiply(Quantity);
}

