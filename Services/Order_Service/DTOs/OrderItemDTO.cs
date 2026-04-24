using Order_Service.Models;

namespace Order_Service.DTOs;

public class OrderItemDTO
{
    public Guid OrderItemId { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Customization { get; set; }
    public decimal Subtotal { get; set; }
     public OrderItemDTO() { }

    public OrderItemDTO(Guid orderItemId, Guid menuItemId, string name, decimal price,
        int quantity, string? customization, decimal subtotal)
    {
        OrderItemId = orderItemId;
        MenuItemId = menuItemId;
        Name = name;
        Price = price;
        Quantity = quantity;
        Customization = customization;
        Subtotal = subtotal;
    }

}
