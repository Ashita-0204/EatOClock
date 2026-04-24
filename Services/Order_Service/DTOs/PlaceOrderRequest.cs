using Order_Service.Models;

namespace Order_Service.DTOs;

public class PlaceOrderRequest
{
    public Guid RestaurantId { get; set; }
    public string ModeOfPayment { get; set; } = string.Empty; // "COD" | "Online"
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? PromoCode { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();

    public PlaceOrderRequest() { }
    public PlaceOrderRequest(Guid restaurantId, string modeOfPayment, string deliveryAddress,
        string? notes, string? promoCode, List<OrderItemRequest> items)
    {
        RestaurantId = restaurantId;
        ModeOfPayment = modeOfPayment;
        DeliveryAddress = deliveryAddress;
        Notes = notes;
        PromoCode = promoCode;
        Items = items;
    }
}
