using DeliveryAgent_Service.Models;

namespace DeliveryAgent_Service.DTOs;

public class DeliveryRecordDTO
{
    public Guid DeliveryId { get; set; }
    public Guid OrderId { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal EarningsForDelivery { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? Rating { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
public DeliveryRecordDTO() { }

    public DeliveryRecordDTO(Guid deliveryId, Guid orderId, string pickupAddress,
        string deliveryAddress, decimal earnings, string status, int? rating,
        DateTime assignedAt, DateTime? pickedUpAt, DateTime? deliveredAt)
    {
        DeliveryId = deliveryId;
        OrderId = orderId;
        PickupAddress = pickupAddress;
        DeliveryAddress = deliveryAddress;
        EarningsForDelivery = earnings;
        Status = status;
        Rating = rating;
        AssignedAt = assignedAt;
        PickedUpAt = pickedUpAt;
        DeliveredAt = deliveredAt;
    }
    }
