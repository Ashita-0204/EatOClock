using Order_Service.Models;

namespace Order_Service.DTOs;

public class OrderItemRequest
{
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Customization { get; set; }
    public OrderItemRequest() { }

    public OrderItemRequest(Guid menuItemId, string name, decimal price, int quantity, string? customization)
    {
        MenuItemId = menuItemId;
        Name = name;
        Price = price;
        Quantity = quantity;
        Customization = customization;
    }
}
