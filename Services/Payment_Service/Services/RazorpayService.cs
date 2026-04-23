using System.Security.Cryptography;
using System.Text;
using Payment_Service.Interfaces;
using Razorpay.Api;

namespace Payment_Service.Services;

public class RazorpayService : IRazorpayService
{
    private readonly string _keyId;
    private readonly string _keySecret;

    public RazorpayService(IConfiguration config)
    {
        _keyId = config["Razorpay:KeyId"] ?? throw new InvalidOperationException("Razorpay KeyId missing");
        _keySecret = config["Razorpay:KeySecret"] ?? throw new InvalidOperationException("Razorpay KeySecret missing");
    }

    public string CreateOrder(decimal amount, string currency, string receipt)
    {
        var client = new RazorpayClient(_keyId, _keySecret);
        var options = new Dictionary<string, object>
        {
            { "amount", (int)(amount * 100) }, // paise
            { "currency", currency },
            { "receipt", receipt }
        };
        var order = client.Order.Create(options);
        return order["id"].ToString()!;
    }

    public bool VerifySignature(string orderId, string paymentId, string signature)
    {
        var payload = $"{orderId}|{paymentId}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var generated = BitConverter.ToString(hash).Replace("-", "").ToLower();
        return generated == signature;
    }

    public void RefundPayment(string razorpayPaymentId, decimal amount)
    {
        var client = new RazorpayClient(_keyId, _keySecret);
        var options = new Dictionary<string, object> { { "amount", (int)(amount * 100) } };
        client.Payment.Fetch(razorpayPaymentId).Refund(options);
    }
}
