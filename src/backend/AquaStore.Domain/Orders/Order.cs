using Common.Domain.Primitives;
using Common.Domain.Events;
using AquaStore.Domain.Enums;
using AquaStore.Domain.Users;
using AquaStore.Domain.ValueObjects;

namespace AquaStore.Domain.Orders;

/// <summary>
/// Заказ
/// </summary>
public sealed class Order : AggregateRoot, IAuditableEntity
{
    public string OrderNumber { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }

    public Address ShippingAddress { get; private set; } = null!;
    public string? CustomerNote { get; private set; }

    public Money SubTotal { get; private set; } = null!;
    public Money ShippingCost { get; private set; } = null!;
    public Money? Discount { get; private set; }
    public Money TotalAmount { get; private set; } = null!;

    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(
        Guid userId,
        Address shippingAddress,
        string? customerNote = null)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            ShippingAddress = shippingAddress,
            CustomerNote = customerNote,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            SubTotal = Money.Zero(),
            ShippingCost = Money.Zero(),
            TotalAmount = Money.Zero(),
            CreatedAtUtc = DateTime.UtcNow
        };

        return order;
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
    }

    public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        var item = OrderItem.Create(Id, productId, productName, unitPrice, quantity);
        _items.Add(item);
        RecalculateTotals();
    }

    public void SetShippingCost(decimal cost)
    {
        ShippingCost = Money.Create(cost);
        RecalculateTotals();
    }

    public void ApplyDiscount(decimal discount)
    {
        Discount = Money.Create(discount);
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        if (_items.Count == 0)
        {
            SubTotal = Money.Zero();
            TotalAmount = ShippingCost;
            return;
        }

        SubTotal = _items
            .Select(i => i.TotalPrice)
            .Aggregate((a, b) => a.Add(b));

        var total = SubTotal.Add(ShippingCost);

        if (Discount is not null)
        {
            total = total.Subtract(Discount);
        }

        TotalAmount = total;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        Status = OrderStatus.Confirmed;
        RaiseDomainEvent(new OrderConfirmedEvent(Id, OrderNumber));
    }

    public void Process()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be processed");

        Status = OrderStatus.Processing;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Only processing orders can be shipped");

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderShippedEvent(Id, OrderNumber));
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be delivered");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderDeliveredEvent(Id, OrderNumber));
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel shipped or delivered orders");

        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledEvent(Id, OrderNumber));
    }

    public void MarkAsPaid()
    {
        PaymentStatus = PaymentStatus.Paid;
    }

    public void MarkPaymentFailed()
    {
        PaymentStatus = PaymentStatus.Failed;
    }

    public void Refund()
    {
        PaymentStatus = PaymentStatus.Refunded;
    }

    public bool CanBeCancelled => Status is not (OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled);
}

/// <summary>
/// Событие подтверждения заказа
/// </summary>
public sealed record OrderConfirmedEvent(Guid OrderId, string OrderNumber) : DomainEvent;

/// <summary>
/// Событие отправки заказа
/// </summary>
public sealed record OrderShippedEvent(Guid OrderId, string OrderNumber) : DomainEvent;

/// <summary>
/// Событие доставки заказа
/// </summary>
public sealed record OrderDeliveredEvent(Guid OrderId, string OrderNumber) : DomainEvent;

/// <summary>
/// Событие отмены заказа
/// </summary>
public sealed record OrderCancelledEvent(Guid OrderId, string OrderNumber) : DomainEvent;

