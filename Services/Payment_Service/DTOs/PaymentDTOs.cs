using Payment_Service.Enums;

namespace Payment_Service.DTOs;

// --- Payment DTOs ---
public record ProcessPaymentRequest(Guid OrderId, decimal Amount, PaymentMode Mode,
    string? RazorpayPaymentId = null, string? RazorpayOrderId = null, string? RazorpaySignature = null);

public record RefundRequest(Guid PaymentId, string Reason = "Order cancelled");

public record PaymentResponse(Guid PaymentId, Guid OrderId, string CustomerId, decimal Amount,
    PaymentStatus Status, PaymentMode Mode, string? RazorpayOrderId, DateTime CreatedAt);

public record RazorpayOrderResponse(string RazorpayOrderId, decimal Amount, string Currency, Guid InternalPaymentId);

// --- Wallet DTOs ---
public record AddMoneyRequest(decimal Amount, string? RazorpayPaymentId = null, string? RazorpayOrderId = null, string? RazorpaySignature = null);

public record WalletPayRequest(Guid OrderId, decimal Amount);

public record WalletResponse(Guid WalletId, string CustomerId, decimal Balance);

public record WalletStatementResponse(Guid StatementId, WalletTransactionType Type, decimal Amount,
    string Description, string? TransactionRef, DateTime CreatedAt);

// --- Razorpay create order ---
public record CreateRazorpayOrderRequest(decimal Amount, string Currency = "INR");
