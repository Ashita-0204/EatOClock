namespace Cart_Service.DTOs;

public class CartDTOs
{
    public Guid CartId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DiscountedTotal { get; set; }
    public string? AppliedPromo { get; set; }
    public List<CartItemDTO> Items { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
    public CartDTOs() { }

    public CartDTOs(Guid cartId, string customerId, Guid restaurantId,
        decimal totalPrice, decimal discountedTotal, string? appliedPromo,
        List<CartItemDTO> items, DateTime updatedAt)
    {
        CartId = cartId;
        CustomerId = customerId;
        RestaurantId = restaurantId;
        TotalPrice = totalPrice;
        DiscountedTotal = discountedTotal;
        AppliedPromo = appliedPromo;
        Items = items;
        UpdatedAt = updatedAt;
    }
}
