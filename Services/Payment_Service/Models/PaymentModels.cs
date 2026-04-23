using Payment_Service.Enums;

namespace Payment_Service.Models;

public class Payment
{
    public Guid PaymentId { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;
    public PaymentMode Mode { get; set; }
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class Wallet
{
    public Guid WalletId { get; set; } = Guid.NewGuid();
    public string CustomerId { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<WalletStatement> Statements { get; set; } = new();
}

public class WalletStatement
{
    public Guid StatementId { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;
    public WalletTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? TransactionRef { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
