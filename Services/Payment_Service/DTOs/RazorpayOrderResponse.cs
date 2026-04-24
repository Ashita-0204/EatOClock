using Payment_Service.Enums;

namespace Payment_Service.DTOs;

public class RazorpayOrderResponse
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid InternalPaymentId { get; set; }

    public RazorpayOrderResponse() { }

    public RazorpayOrderResponse(string razorpayOrderId, decimal amount, string currency, Guid internalPaymentId)
    {
        RazorpayOrderId = razorpayOrderId;
        Amount = amount;
        Currency = currency;
        InternalPaymentId = internalPaymentId;
    }
}
