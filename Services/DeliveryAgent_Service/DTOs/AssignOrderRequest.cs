using DeliveryAgent_Service.Models;

namespace DeliveryAgent_Service.DTOs;

public class AssignOrderRequest
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal EarningsForDelivery { get; set; }
    public AssignOrderRequest() { }

    public AssignOrderRequest(Guid orderId, string customerId,
        string pickupAddress, string deliveryAddress, decimal earningsForDelivery)
    {
        OrderId = orderId;
        CustomerId = customerId;
        PickupAddress = pickupAddress;
        DeliveryAddress = deliveryAddress;
        EarningsForDelivery = earningsForDelivery;
    }
}
