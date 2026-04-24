using Order_Service.Models;

namespace Order_Service.DTOs;

public class OrderDTOs
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }
    public string? DeliveryAgentId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalAmount { get; set; }
    public string ModeOfPayment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public List<OrderItemDTO> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
public OrderDTOs() { }

    public OrderDTOs(Guid orderId, string customerId, Guid restaurantId, string? deliveryAgentId,
        decimal totalAmount, decimal discount, decimal finalAmount, string modeOfPayment,
        string status, string deliveryAddress, string? notes, string? cancellationReason,
        List<OrderItemDTO> items, DateTime createdAt, DateTime updatedAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        RestaurantId = restaurantId;
        DeliveryAgentId = deliveryAgentId;
        TotalAmount = totalAmount;
        Discount = discount;
        FinalAmount = finalAmount;
        ModeOfPayment = modeOfPayment;
        Status = status;
        DeliveryAddress = deliveryAddress;
        Notes = notes;
        CancellationReason = cancellationReason;
        Items = items;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
