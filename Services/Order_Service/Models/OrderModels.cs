namespace Order_Service.Models;

public class Order
{
    public Guid OrderId { get; set; } = Guid.NewGuid();
    public string CustomerId { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }
    public string? DeliveryAgentId { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalAmount { get; set; }

    public string ModeOfPayment { get; set; } = "COD"; // COD | Online
    public OrderStatus Status { get; set; } = OrderStatus.PLACED;

    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public Guid OrderItemId { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }

    // Immutable snapshot
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Customization { get; set; }

    public Order Order { get; set; } = null!;
}
