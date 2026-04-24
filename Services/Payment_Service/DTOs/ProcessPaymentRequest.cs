using Payment_Service.Enums;

namespace Payment_Service.DTOs;

// --- Payment DTOs ---

public class ProcessPaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMode Mode { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpayOrderId { get; set; }
    public string? RazorpaySignature { get; set; }

    public ProcessPaymentRequest() { }

    public ProcessPaymentRequest(Guid orderId, decimal amount, PaymentMode mode,
        string? razorpayPaymentId = null, string? razorpayOrderId = null, string? razorpaySignature = null)
    {
        OrderId = orderId;
        Amount = amount;
        Mode = mode;
        RazorpayPaymentId = razorpayPaymentId;
        RazorpayOrderId = razorpayOrderId;
        RazorpaySignature = razorpaySignature;
    }
}
