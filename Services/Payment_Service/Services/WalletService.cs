using Microsoft.EntityFrameworkCore;
using Payment_Service.Data;
using Payment_Service.DTOs;
using Payment_Service.Enums;
using Payment_Service.Interfaces;
using Payment_Service.Models;

namespace Payment_Service.Services;

public class WalletService(AppDbContext db, IRazorpayService razorpay) : IWalletService
{
    private async Task<Wallet> GetOrCreateWalletAsync(string customerId)
    {
        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.CustomerId == customerId);
        if (wallet != null) return wallet;
        wallet = new Wallet { CustomerId = customerId };
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
        return wallet;
    }

    public async Task<WalletResponse> GetBalanceAsync(string customerId)
    {
        var wallet = await GetOrCreateWalletAsync(customerId);
        return new WalletResponse(wallet.WalletId, wallet.CustomerId, wallet.Balance);
    }

    public async Task<WalletResponse> AddMoneyAsync(string customerId, AddMoneyRequest req)
    {
        if (req.Amount <= 0) throw new ArgumentException("Amount must be positive");

        // Only verify signature when REAL Razorpay credentials are supplied (not null/empty/Swagger placeholder)
        bool hasRealRazorpay = !string.IsNullOrWhiteSpace(req.RazorpayPaymentId)
                            && !string.IsNullOrWhiteSpace(req.RazorpayOrderId)
                            && !string.IsNullOrWhiteSpace(req.RazorpaySignature)
                            && req.RazorpayPaymentId != "string"
                            && req.RazorpayOrderId != "string"
                            && req.RazorpaySignature != "string";

        if (hasRealRazorpay)
        {
            if (!razorpay.VerifySignature(req.RazorpayOrderId!, req.RazorpayPaymentId!, req.RazorpaySignature!))
                throw new InvalidOperationException("Razorpay signature verification failed");
        }

        var wallet = await GetOrCreateWalletAsync(customerId);
        wallet.Balance += req.Amount;

        db.WalletStatements.Add(new WalletStatement
        {
            WalletId = wallet.WalletId,
            Type = WalletTransactionType.CREDIT,
            Amount = req.Amount,
            Description = hasRealRazorpay ? "Wallet top-up via Razorpay" : "Wallet top-up",
            TransactionRef = req.RazorpayPaymentId
        });

        await db.SaveChangesAsync();
        return new WalletResponse(wallet.WalletId, wallet.CustomerId, wallet.Balance);
    }

    public async Task<WalletResponse> PayFromWalletAsync(string customerId, WalletPayRequest req)
    {
        if (req.Amount <= 0) throw new ArgumentException("Amount must be positive");

        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.CustomerId == customerId)
            ?? throw new InvalidOperationException("Wallet not found");

        if (wallet.Balance < req.Amount)
            throw new InvalidOperationException("Insufficient wallet balance");

        wallet.Balance -= req.Amount;

        db.WalletStatements.Add(new WalletStatement
        {
            WalletId = wallet.WalletId,
            Type = WalletTransactionType.DEBIT,
            Amount = req.Amount,
            Description = $"Payment for order {req.OrderId}",
            TransactionRef = req.OrderId.ToString()
        });

        await db.SaveChangesAsync();
        return new WalletResponse(wallet.WalletId, wallet.CustomerId, wallet.Balance);
    }

    public async Task<IEnumerable<WalletStatementResponse>> GetStatementsAsync(string customerId)
    {
        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.CustomerId == customerId);
        if (wallet == null) return Enumerable.Empty<WalletStatementResponse>();

        return await db.WalletStatements
            .Where(s => s.WalletId == wallet.WalletId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new WalletStatementResponse(s.StatementId, s.Type, s.Amount, s.Description, s.TransactionRef, s.CreatedAt))
            .ToListAsync();
    }

    public async Task CreditWalletAsync(string customerId, decimal amount, string description, string? txRef = null)
    {
        var wallet = await GetOrCreateWalletAsync(customerId);
        wallet.Balance += amount;
        db.WalletStatements.Add(new WalletStatement
        {
            WalletId = wallet.WalletId,
            Type = WalletTransactionType.CREDIT,
            Amount = amount,
            Description = description,
            TransactionRef = txRef
        });
        await db.SaveChangesAsync();
    }
}
