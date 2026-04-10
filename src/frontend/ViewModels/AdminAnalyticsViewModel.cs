namespace frontend.ViewModels;

public sealed class AdminOrderAnalyticsViewModel
{
    public DateTime GeneratedAtUtc { get; set; }
    public string Currency { get; set; } = "BYN";
    public int TotalOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int UniqueCustomers { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<OrderStatusAnalyticsItemViewModel> StatusBreakdown { get; set; } = new();
    public List<ProductAnalyticsItemViewModel> TopProductsByQuantity { get; set; } = new();
    public List<ProductAnalyticsItemViewModel> TopProductsByRevenue { get; set; } = new();
    public List<UserAnalyticsItemViewModel> TopUsersBySpend { get; set; } = new();
    public List<UserAnalyticsItemViewModel> TopUsersByOrdersCount { get; set; } = new();
}

public sealed class OrderStatusAnalyticsItemViewModel
{
    public int Status { get; set; }
    public int Count { get; set; }
    public decimal SharePercent { get; set; }
}

public sealed class ProductAnalyticsItemViewModel
{
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
    public int OrdersCount { get; set; }
}

public sealed class UserAnalyticsItemViewModel
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageOrderValue { get; set; }
}
