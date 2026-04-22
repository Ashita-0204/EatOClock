using Microsoft.EntityFrameworkCore;
using Payment_Service.Models;

namespace Payment_Service.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletStatement> WalletStatements => Set<WalletStatement>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Payment>().HasKey(p => p.PaymentId);
        mb.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);

        mb.Entity<Wallet>().HasKey(w => w.WalletId);
        mb.Entity<Wallet>().HasIndex(w => w.CustomerId).IsUnique();
        mb.Entity<Wallet>().Property(w => w.Balance).HasPrecision(18, 2);

        mb.Entity<WalletStatement>().HasKey(s => s.StatementId);
        mb.Entity<WalletStatement>().Property(s => s.Amount).HasPrecision(18, 2);
        mb.Entity<WalletStatement>()
            .HasOne(s => s.Wallet)
            .WithMany(w => w.Statements)
            .HasForeignKey(s => s.WalletId);
    }
}
