using Payment_Service.Enums;

namespace Payment_Service.DTOs;

public class WalletResponse
{
    public Guid WalletId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal Balance { get; set; }

    public WalletResponse() { }

    public WalletResponse(Guid walletId, string customerId, decimal balance)
    {
        WalletId = walletId;
        CustomerId = customerId;
        Balance = balance;
    }
}
