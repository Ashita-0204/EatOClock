using Microsoft.EntityFrameworkCore;
using Payment_Service.Data;
using Payment_Service.DTOs;
using Payment_Service.Enums;
using Payment_Service.Interfaces;
using Payment_Service.Models;

namespace Payment_Service.Services;

public class PaymentService(AppDbContext db, IRazorpayService razorpay, IWalletService walletService) : IPaymentService
{
    public async Task<(PaymentResponse payment, string? razorpayOrderId)> ProcessPaymentAsync(string customerId, ProcessPaymentRequest req)
    {
        string? rzpOrderId = null;

        var payment = new Payment
        {
            OrderId = req.OrderId,
            CustomerId = customerId,
            Amount = req.Amount,
            Mode = req.Mode
        };

        switch (req.Mode)
        {
            case PaymentMode.COD:
                payment.Status = PaymentStatus.PAID;
                break;

            case PaymentMode.WALLET:
                // Deduct from wallet; WalletPayRequest validated inside
                await walletService.PayFromWalletAsync(customerId, new WalletPayRequest(req.OrderId, req.Amount));
                payment.Status = PaymentStatus.PAID;
                break;

            case PaymentMode.CARD:
            case PaymentMode.UPI:
                if (req.RazorpayPaymentId == null || req.RazorpayOrderId == null || req.RazorpaySignature == null)
                {
                    // First call: create Razorpay order
                    rzpOrderId = razorpay.CreateOrder(req.Amount, "INR", req.OrderId.ToString());
                    payment.Status = PaymentStatus.PENDING;
                    payment.RazorpayOrderId = rzpOrderId;
                }
                else
                {
                    // Second call: verify payment
                    if (!razorpay.VerifySignature(req.RazorpayOrderId, req.RazorpayPaymentId, req.RazorpaySignature))
                    {
                        payment.Status = PaymentStatus.FAILED;
                        payment.FailureReason = "Signature verification failed";
                    }
                    else
                    {
                        payment.Status = PaymentStatus.PAID;
                        payment.RazorpayOrderId = req.RazorpayOrderId;
                        payment.RazorpayPaymentId = req.RazorpayPaymentId;
                    }
                }
                break;
        }

        payment.UpdatedAt = DateTime.UtcNow;
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        return (ToResponse(payment), rzpOrderId);
    }

    public async Task<PaymentResponse> RefundPaymentAsync(string requesterId, bool isAdmin, RefundRequest req)
    {
        var payment = await db.Payments.FindAsync(req.PaymentId)
            ?? throw new KeyNotFoundException("Payment not found");

        if (!isAdmin && payment.CustomerId != requesterId)
            throw new UnauthorizedAccessException("Access denied");

        if (payment.Status != PaymentStatus.PAID)
            throw new InvalidOperationException("Only PAID payments can be refunded");

        if (payment.Mode is PaymentMode.CARD or PaymentMode.UPI && payment.RazorpayPaymentId != null)
            razorpay.RefundPayment(payment.RazorpayPaymentId, payment.Amount);

        if (payment.Mode == PaymentMode.WALLET)
            await walletService.CreditWalletAsync(payment.CustomerId, payment.Amount, $"Refund: {req.Reason}", payment.PaymentId.ToString());

        payment.Status = PaymentStatus.REFUNDED;
        payment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return ToResponse(payment);
    }

    public async Task<PaymentResponse?> GetByOrderIdAsync(string customerId, Guid orderId)
    {
        var p = await db.Payments.FirstOrDefaultAsync(x => x.OrderId == orderId && x.CustomerId == customerId);
        return p == null ? null : ToResponse(p);
    }

    public async Task<IEnumerable<PaymentResponse>> GetCustomerHistoryAsync(string customerId) =>
        await db.Payments.Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => ToResponse(p)).ToListAsync();

    public async Task<IEnumerable<PaymentResponse>> GetAllTransactionsAsync() =>
        await db.Payments.OrderByDescending(p => p.CreatedAt)
            .Select(p => ToResponse(p)).ToListAsync();

    private static PaymentResponse ToResponse(Payment p) =>
        new(p.PaymentId, p.OrderId, p.CustomerId, p.Amount, p.Status, p.Mode, p.RazorpayOrderId, p.CreatedAt);
}
