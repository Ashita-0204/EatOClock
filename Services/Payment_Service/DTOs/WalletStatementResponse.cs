using Payment_Service.Enums;

namespace Payment_Service.DTOs;

public class WalletStatementResponse
{
    public Guid StatementId { get; set; }
    public WalletTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? TransactionRef { get; set; }
    public DateTime CreatedAt { get; set; }

    public WalletStatementResponse() { }

    public WalletStatementResponse(Guid statementId, WalletTransactionType type,
        decimal amount, string description, string? transactionRef, DateTime createdAt)
    {
        StatementId = statementId;
        Type = type;
        Amount = amount;
        Description = description;
        TransactionRef = transactionRef;
        CreatedAt = createdAt;
    }
}
