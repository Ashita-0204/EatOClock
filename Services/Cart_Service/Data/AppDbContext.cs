using Cart_Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Cart_Service.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Cart>(e =>
        {
            e.HasKey(c => c.CartId);
            e.Property(c => c.TotalPrice).HasPrecision(18, 2);
            e.HasIndex(c => c.CustomerId).IsUnique(); // one cart per customer
        });

        mb.Entity<CartItem>(e =>
        {
            e.HasKey(i => i.ItemId);
            e.Property(i => i.Price).HasPrecision(18, 2);
            e.HasOne(i => i.Cart)
             .WithMany(c => c.Items)
             .HasForeignKey(i => i.CartId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<PromoCode>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.Code).IsUnique();
            e.Property(p => p.DiscountPercent).HasPrecision(5, 2);

            // Fixed dates for deterministic migrations (no DateTime.UtcNow in seed data)
            e.HasData(
                new PromoCode { Id = Guid.Parse("11111111-0000-0000-0000-000000000001"), Code = "SAVE10", DiscountPercent = 10, IsActive = true, ExpiresAt = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new PromoCode { Id = Guid.Parse("11111111-0000-0000-0000-000000000002"), Code = "SAVE20", DiscountPercent = 20, IsActive = true, ExpiresAt = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });
    }
}
