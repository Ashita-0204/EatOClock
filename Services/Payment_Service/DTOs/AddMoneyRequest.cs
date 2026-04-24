using Payment_Service.Enums;

namespace Payment_Service.DTOs;

public class AddMoneyRequest
{
    public decimal Amount { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpayOrderId { get; set; }
    public string? RazorpaySignature { get; set; }

    public AddMoneyRequest() { }

    public AddMoneyRequest(decimal amount, string? razorpayPaymentId = null,
        string? razorpayOrderId = null, string? razorpaySignature = null)
    {
        Amount = amount;
        RazorpayPaymentId = razorpayPaymentId;
        RazorpayOrderId = razorpayOrderId;
        RazorpaySignature = razorpaySignature;
    }
}
