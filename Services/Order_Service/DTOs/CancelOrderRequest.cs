using Order_Service.Models;

namespace Order_Service.DTOs;

public class CancelOrderRequest
{
    public string? Reason { get; set; }
    public CancelOrderRequest() { }

    public CancelOrderRequest(string? reason)
    {
        Reason = reason;
    }
}
