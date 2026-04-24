using Payment_Service.Enums;

namespace Payment_Service.DTOs;

public class CreateRazorpayOrderRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";

    public CreateRazorpayOrderRequest() { }

    public CreateRazorpayOrderRequest(decimal amount, string currency = "INR")
    {
        Amount = amount;
        Currency = currency;
    }
}