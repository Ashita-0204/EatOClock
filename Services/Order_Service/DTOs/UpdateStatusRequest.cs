using System.Text.Json.Serialization;
using Order_Service.Models;

namespace Order_Service.DTOs;

public class UpdateStatusRequest
{
    [JsonPropertyName("status")]
    public OrderStatus Status { get; set; }
    
     public UpdateStatusRequest() { }
    public UpdateStatusRequest(OrderStatus status)
    {
        Status = status;
    }
}
