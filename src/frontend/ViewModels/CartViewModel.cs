namespace frontend.ViewModels;

public class CartViewModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "BYN";
    public int TotalItems { get; set; }
}

public class CartItemViewModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

