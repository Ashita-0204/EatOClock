using Payment_Service.Enums;

namespace Payment_Service.DTOs;

public class WalletPayRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }

    public WalletPayRequest() { }

    public WalletPayRequest(Guid orderId, decimal amount)
    {
        OrderId = orderId;
        Amount = amount;
    }
}
