using Microsoft.EntityFrameworkCore;
using Payment_Service.Data;
using Payment_Service.DTOs;
using Payment_Service.Enums;
using Payment_Service.Interfaces;
using Payment_Service.Models;

namespace Payment_Service.Services;

public class PaymentService(AppDbContext db, IRazorpayService razorpay, IWalletService walletService)
    : IPaymentService
{
    // -- Helpers --------------------------------------------------------------

    private static PaymentResponse ToResponse(Payment p) =>
        new(p.PaymentId, p.OrderId, p.CustomerId, p.Amount, p.Status,
            p.Mode, p.RazorpayOrderId, p.CreatedAt);

    private Task<Payment?> FetchAsync(Guid paymentId) =>
        db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.PaymentId == paymentId);

    // -- Process payment ------------------------------------------------------

    public async Task<(PaymentResponse payment, string? razorpayOrderId)>
        ProcessPaymentAsync(string customerId, ProcessPaymentRequest req)
    {
        string? rzpOrderId = null;
        var now = DateTime.UtcNow;

        Payment payment;

        // Determine if this is a verification call (second step)
        bool isVerificationCall = (req.Mode == PaymentMode.CARD || req.Mode == PaymentMode.UPI) && 
                                  req.RazorpayPaymentId != null && 
                                  req.RazorpayOrderId != null && 
                                  req.RazorpaySignature != null;

        if (isVerificationCall)
        {
            // Fetch existing payment record
            var existingPayment = await db.Payments
                .FirstOrDefaultAsync(p => p.OrderId == req.OrderId && p.RazorpayOrderId == req.RazorpayOrderId);
                
            if (existingPayment == null)
                throw new InvalidOperationException("Payment record not found for verification.");
                
            payment = existingPayment;
            payment.UpdatedAt = now;
        }
        else
        {
            // Create a new payment record
            payment = new Payment
            {
                OrderId    = req.OrderId,
                CustomerId = customerId,
                Amount     = req.Amount,
                Mode       = req.Mode,
                UpdatedAt  = now
            };
        }

        switch (req.Mode)
        {
            case PaymentMode.COD:
                payment.Status = PaymentStatus.PAID;
                break;

            case PaymentMode.WALLET:
                // Deduct from wallet first; throws on insufficient balance.
                await walletService.PayFromWalletAsync(
                    customerId, new WalletPayRequest(req.OrderId, req.Amount));
                payment.Status = PaymentStatus.PAID;
                break;

            case PaymentMode.CARD:
            case PaymentMode.UPI:
                if (!isVerificationCall)
                {
                    // First call: create a Razorpay order.
                    try 
                    {
                        rzpOrderId                = razorpay.CreateOrder(req.Amount, "INR", req.OrderId.ToString());
                        payment.Status            = PaymentStatus.PENDING;
                        payment.RazorpayOrderId   = rzpOrderId;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Razorpay gateway error: {ex.Message}");
                    }
                }
                else
                {
                    // Second call: verify the payment signature.
                    bool valid = razorpay.VerifySignature(
                        req.RazorpayOrderId!, req.RazorpayPaymentId!, req.RazorpaySignature!);

                    if (!valid)
                    {
                        payment.Status        = PaymentStatus.FAILED;
                        payment.FailureReason = "Signature verification failed";
                    }
                    else
                    {
                        payment.Status             = PaymentStatus.PAID;
                        payment.RazorpayOrderId    = req.RazorpayOrderId;
                        payment.RazorpayPaymentId  = req.RazorpayPaymentId;
                    }
                }
                break;
        }

        if (!isVerificationCall)
        {
            db.Payments.Add(payment);
        }
        
        await db.SaveChangesAsync();

        return (ToResponse(payment), rzpOrderId);
    }

    // -- Refund ---------------------------------------------------------------

    public async Task<PaymentResponse> RefundPaymentAsync(
        string requesterId, bool isAdmin, RefundRequest req)
    {
        var payment = await FetchAsync(req.PaymentId)
                      ?? throw new KeyNotFoundException("Payment not found.");

        if (!isAdmin && payment.CustomerId != requesterId)
            throw new UnauthorizedAccessException("Access denied.");

        if (payment.Status != PaymentStatus.PAID)
            throw new InvalidOperationException("Only PAID payments can be refunded.");

        // Trigger external refunds first (before we update the DB row).
        if (payment.Mode is PaymentMode.CARD or PaymentMode.UPI &&
            payment.RazorpayPaymentId != null)
            razorpay.RefundPayment(payment.RazorpayPaymentId, payment.Amount);

        if (payment.Mode == PaymentMode.WALLET)
            await walletService.CreditWalletAsync(
                payment.CustomerId, payment.Amount,
                $"Refund: {req.Reason}", payment.PaymentId.ToString());

        // FIX: targeted UPDATE — no tracked entity, no concurrency token check.
        await db.Payments
            .Where(p => p.PaymentId == payment.PaymentId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Status,    PaymentStatus.REFUNDED)
                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));

        // Re-read to return the up-to-date record.
        return ToResponse((await FetchAsync(payment.PaymentId))!);
    }

    // -- Queries --------------------------------------------------------------

    public async Task<PaymentResponse?> GetByOrderIdAsync(string customerId, Guid orderId)
    {
        var p = await db.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderId == orderId && x.CustomerId == customerId);
        return p == null ? null : ToResponse(p);
    }

    public async Task<IEnumerable<PaymentResponse>> GetCustomerHistoryAsync(string customerId) =>
        await db.Payments
            .AsNoTracking()
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => ToResponse(p))
            .ToListAsync();

    public async Task<IEnumerable<PaymentResponse>> GetAllTransactionsAsync() =>
        await db.Payments
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => ToResponse(p))
            .ToListAsync();
}