using Order_Service.Models;
namespace Order_Service.DTOs;

public class AssignAgentRequest
{
    public string DeliveryAgentId { get; set; } = string.Empty;
    public AssignAgentRequest() { }

    public AssignAgentRequest(string deliveryAgentId)
    {
        DeliveryAgentId = deliveryAgentId;
    }
}
