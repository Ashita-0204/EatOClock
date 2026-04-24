using Payment_Service.DTOs;
using Payment_Service.Models;

namespace Payment_Service.Interfaces;

public interface IPaymentService
{
    Task<(PaymentResponse payment, string? razorpayOrderId)> ProcessPaymentAsync(string customerId, ProcessPaymentRequest req);
    Task<PaymentResponse> RefundPaymentAsync(string requesterId, bool isAdmin, RefundRequest req);
    Task<PaymentResponse?> GetByOrderIdAsync(string customerId, Guid orderId);
    Task<IEnumerable<PaymentResponse>> GetCustomerHistoryAsync(string customerId);
    Task<IEnumerable<PaymentResponse>> GetAllTransactionsAsync();
}

public interface IWalletService
{
    Task<WalletResponse> GetBalanceAsync(string customerId);
    Task<WalletResponse> AddMoneyAsync(string customerId, AddMoneyRequest req);
    Task<WalletResponse> PayFromWalletAsync(string customerId, WalletPayRequest req);
    Task<IEnumerable<WalletStatementResponse>> GetStatementsAsync(string customerId);
    Task CreditWalletAsync(string customerId, decimal amount, string description, string? txRef = null);
}

public interface IRazorpayService
{
    string CreateOrder(decimal amount, string currency, string receipt);
    bool VerifySignature(string orderId, string paymentId, string signature);
    void RefundPayment(string razorpayPaymentId, decimal amount);
}
