using Payment_Service.Enums;

namespace Payment_Service.DTOs;

public class RefundRequest
{
    public Guid PaymentId { get; set; }
    public string Reason { get; set; } = "Order cancelled";

    public RefundRequest() { }

    public RefundRequest(Guid paymentId, string reason = "Order cancelled")
    {
        PaymentId = paymentId;
        Reason = reason;
    }
}
