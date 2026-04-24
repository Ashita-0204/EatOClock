using Payment_Service.Enums;

namespace Payment_Service.DTOs;

public class PaymentResponse
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMode Mode { get; set; }
    public string? RazorpayOrderId { get; set; }
    public DateTime CreatedAt { get; set; }

    public PaymentResponse() { }

    public PaymentResponse(Guid paymentId, Guid orderId, string customerId, decimal amount,
        PaymentStatus status, PaymentMode mode, string? razorpayOrderId, DateTime createdAt)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
        Status = status;
        Mode = mode;
        RazorpayOrderId = razorpayOrderId;
        CreatedAt = createdAt;
    }
}
