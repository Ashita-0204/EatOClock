namespace Payment_Service.Enums;

public enum PaymentStatus { PENDING, PAID, REFUNDED, FAILED }
public enum PaymentMode { COD, CARD, UPI, WALLET }
public enum WalletTransactionType { CREDIT, DEBIT }
