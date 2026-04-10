using System.ComponentModel.DataAnnotations;

namespace frontend.ViewModels;

public sealed class CreateOrderViewModel
{
    [Required(ErrorMessage = "Укажите город")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите улицу")]
    [StringLength(150)]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите дом")]
    [StringLength(50)]
    public string Building { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Apartment { get; set; }

    [Required(ErrorMessage = "Укажите почтовый индекс")]
    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [StringLength(500)]
    public string? CustomerNote { get; set; }

    public List<Guid> SelectedProductIds { get; set; } = new();
    public bool BuyNowSingleUnit { get; set; }
}

public sealed class CheckoutViewModel
{
    public CartViewModel Cart { get; set; } = new();
    public CreateOrderViewModel Order { get; set; } = new();
    public List<UserAddressViewModel> SavedAddresses { get; set; } = new();
    public Guid? SelectedAddressId { get; set; }
    public bool UseManualAddress { get; set; } = true;
    public bool SaveAddressToProfile { get; set; }
}

public sealed class CreateOrderResponseViewModel
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
}

public sealed class OrderListItemViewModel
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int Status { get; set; }
    public int PaymentStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "BYN";
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class OrderDetailViewModel
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int Status { get; set; }
    public int PaymentStatus { get; set; }
    public OrderAddressViewModel ShippingAddress { get; set; } = new();
    public string? CustomerNote { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal? Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "BYN";
    public List<OrderItemViewModel> Items { get; set; } = new();
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class OrderAddressViewModel
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public string? Apartment { get; set; }
    public string PostalCode { get; set; } = string.Empty;
}

public sealed class OrderItemViewModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public sealed class AdminOrderListItemViewModel
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int Status { get; set; }
    public int PaymentStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "BYN";
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

