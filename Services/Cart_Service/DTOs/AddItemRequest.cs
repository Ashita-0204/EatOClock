namespace Cart_Service.DTOs;

// --- Requests ---------------------------------------------------------------

public class AddItemRequest
{
    public Guid RestaurantId { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Customization { get; set; } = null;
public AddItemRequest() { }

    public AddItemRequest(Guid restaurantId, Guid menuItemId, string name,
        decimal price, int quantity, string? customization)
    {
        RestaurantId = restaurantId;
        MenuItemId = menuItemId;
        Name = name;
        Price = price;
        Quantity = quantity;
        Customization = customization;
    }
}