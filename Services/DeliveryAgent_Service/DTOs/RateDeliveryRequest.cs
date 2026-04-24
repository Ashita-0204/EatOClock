using DeliveryAgent_Service.Models;

namespace DeliveryAgent_Service.DTOs;

public class RateDeliveryRequest
{
    public Guid DeliveryId { get; set; }
    public int Rating { get; set; }
    public string? Note { get; set; }
    public RateDeliveryRequest() { }

    public RateDeliveryRequest(Guid deliveryId, int rating, string? note)
    {
        DeliveryId = deliveryId;
        Rating = rating;
        Note = note;
    }
}
