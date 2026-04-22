namespace Cart_Service.Models;

public class Cart
{
    public Guid CartId { get; set; } = Guid.NewGuid();
    public string CustomerId { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<CartItem> Items { get; set; } = new();
}

public class CartItem
{
    public Guid ItemId { get; set; } = Guid.NewGuid();
    public Guid CartId { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;       // snapshot
    public decimal Price { get; set; }                      // snapshot
    public int Quantity { get; set; }
    public string? Customization { get; set; }

    public Cart Cart { get; set; } = null!;
}

public class PromoCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime ExpiresAt { get; set; }
}
