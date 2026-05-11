using Microsoft.EntityFrameworkCore;
using Order_Service.Models;

namespace Order_Service.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.HasDefaultSchema("orders");
        mb.Entity<Order>(e =>
        {
            e.HasKey(o => o.OrderId);
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.Discount).HasColumnType("decimal(18,2)");
            e.Property(o => o.FinalAmount).HasColumnType("decimal(18,2)");
            e.HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<OrderItem>(e =>
        {
            e.HasKey(i => i.OrderItemId);
            e.Property(i => i.Price).HasColumnType("decimal(18,2)");
        });
    }
}
