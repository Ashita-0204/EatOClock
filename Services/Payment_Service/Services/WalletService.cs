using Microsoft.EntityFrameworkCore;
using Payment_Service.Data;
using Payment_Service.DTOs;
using Payment_Service.Enums;
using Payment_Service.Interfaces;
using Payment_Service.Models;

namespace Payment_Service.Services;

public class WalletService(AppDbContext db, IRazorpayService razorpay) : IWalletService
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the wallet ID for this customer, creating the wallet row if it
    /// doesn't exist yet. Uses a plain INSERT so there is no stale tracker state.
    /// </summary>
    private async Task<Guid> EnsureWalletAsync(string customerId)
    {
        // AsNoTracking read — we never mutate via the tracker.
        var existing = await db.Wallets
            .AsNoTracking()
            .Where(w => w.CustomerId == customerId)
            .Select(w => new { w.WalletId })
            .FirstOrDefaultAsync();

        if (existing != null) return existing.WalletId;

        var wallet = new Wallet { CustomerId = customerId };
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
        return wallet.WalletId;
    }

    private async Task<(Guid walletId, decimal balance)> GetWalletDataAsync(string customerId)
    {
        var data = await db.Wallets
            .AsNoTracking()
            .Where(w => w.CustomerId == customerId)
            .Select(w => new { w.WalletId, w.Balance })
            .FirstOrDefaultAsync();

        return data == null ? (Guid.Empty, 0m) : (data.WalletId, data.Balance);
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public async Task<WalletResponse> GetBalanceAsync(string customerId)
    {
        var walletId = await EnsureWalletAsync(customerId);
        var (_, balance) = await GetWalletDataAsync(customerId);
        return new WalletResponse(walletId, customerId, balance);
    }

    public async Task<WalletResponse> AddMoneyAsync(string customerId, AddMoneyRequest req)
    {
        if (req.Amount <= 0) throw new ArgumentException("Amount must be positive.");

        bool hasRealRazorpay = !string.IsNullOrWhiteSpace(req.RazorpayPaymentId)
                            && !string.IsNullOrWhiteSpace(req.RazorpayOrderId)
                            && !string.IsNullOrWhiteSpace(req.RazorpaySignature)
                            && req.RazorpayPaymentId != "string"
                            && req.RazorpayOrderId   != "string"
                            && req.RazorpaySignature  != "string";

        if (hasRealRazorpay &&
            !razorpay.VerifySignature(req.RazorpayOrderId!, req.RazorpayPaymentId!, req.RazorpaySignature!))
            throw new InvalidOperationException("Razorpay signature verification failed.");

        var walletId = await EnsureWalletAsync(customerId);
        var (_, currentBalance) = await GetWalletDataAsync(customerId);
        var newBalance = currentBalance + req.Amount;

        // FIX: targeted UPDATE — no tracked entity, no concurrency token check.
        await db.Wallets
            .Where(w => w.WalletId == walletId)
            .ExecuteUpdateAsync(s => s.SetProperty(w => w.Balance, newBalance));

        // Statement INSERT is always safe.
        db.WalletStatements.Add(new WalletStatement
        {
            WalletId       = walletId,
            Type           = WalletTransactionType.CREDIT,
            Amount         = req.Amount,
            Description    = hasRealRazorpay ? "Wallet top-up via Razorpay" : "Wallet top-up",
            TransactionRef = req.RazorpayPaymentId
        });
        await db.SaveChangesAsync();

        return new WalletResponse(walletId, customerId, newBalance);
    }

    public async Task<WalletResponse> PayFromWalletAsync(string customerId, WalletPayRequest req)
    {
        if (req.Amount <= 0) throw new ArgumentException("Amount must be positive.");

        var (walletId, balance) = await GetWalletDataAsync(customerId);

        if (walletId == Guid.Empty)
            throw new InvalidOperationException("Wallet not found.");

        if (balance < req.Amount)
            throw new InvalidOperationException(
                $"Insufficient wallet balance. Available: {balance:C}, Required: {req.Amount:C}");

        var newBalance = balance - req.Amount;

        await db.Wallets
            .Where(w => w.WalletId == walletId)
            .ExecuteUpdateAsync(s => s.SetProperty(w => w.Balance, newBalance));

        db.WalletStatements.Add(new WalletStatement
        {
            WalletId       = walletId,
            Type           = WalletTransactionType.DEBIT,
            Amount         = req.Amount,
            Description    = $"Payment for order {req.OrderId}",
            TransactionRef = req.OrderId.ToString()
        });
        await db.SaveChangesAsync();

        return new WalletResponse(walletId, customerId, newBalance);
    }

    public async Task<IEnumerable<WalletStatementResponse>> GetStatementsAsync(string customerId)
    {
        var data = await db.Wallets
            .AsNoTracking()
            .Where(w => w.CustomerId == customerId)
            .Select(w => new { w.WalletId })
            .FirstOrDefaultAsync();

        if (data == null) return Enumerable.Empty<WalletStatementResponse>();

        return await db.WalletStatements
            .AsNoTracking()
            .Where(s => s.WalletId == data.WalletId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new WalletStatementResponse(
                s.StatementId, s.Type, s.Amount,
                s.Description, s.TransactionRef, s.CreatedAt))
            .ToListAsync();
    }

    public async Task CreditWalletAsync(
        string customerId, decimal amount, string description, string? txRef = null)
    {
        var walletId = await EnsureWalletAsync(customerId);
        var (_, balance) = await GetWalletDataAsync(customerId);

        await db.Wallets
            .Where(w => w.WalletId == walletId)
            .ExecuteUpdateAsync(s => s.SetProperty(w => w.Balance, balance + amount));

        db.WalletStatements.Add(new WalletStatement
        {
            WalletId       = walletId,
            Type           = WalletTransactionType.CREDIT,
            Amount         = amount,
            Description    = description,
            TransactionRef = txRef
        });
        await db.SaveChangesAsync();
    }
}