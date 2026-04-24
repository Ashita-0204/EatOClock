namespace Cart_Service.DTOs;

public class CartItemDTO
{
    public Guid ItemId { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Customization { get; set; }
    public decimal Subtotal { get; set; }
    public CartItemDTO() { }

    public CartItemDTO(Guid itemId, Guid menuItemId, string name, decimal price,
        int quantity, string? customization, decimal subtotal)
    {
        ItemId = itemId;
        MenuItemId = menuItemId;
        Name = name;
        Price = price;
        Quantity = quantity;
        Customization = customization;
        Subtotal = subtotal;
    }
}