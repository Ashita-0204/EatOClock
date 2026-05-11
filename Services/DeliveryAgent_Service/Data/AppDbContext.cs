using Microsoft.EntityFrameworkCore;
using DeliveryAgent_Service.Models;

namespace DeliveryAgent_Service.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DeliveryAgent> DeliveryAgents => Set<DeliveryAgent>();
    public DbSet<DeliveryRecord> DeliveryRecords => Set<DeliveryRecord>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.HasDefaultSchema("delivery");
        mb.Entity<DeliveryAgent>(e =>
        {
            e.HasKey(a => a.AgentId);
            e.HasIndex(a => a.UserId).IsUnique();
            e.HasIndex(a => a.Phone).IsUnique();
            e.Property(a => a.TotalEarnings).HasPrecision(18, 2);
        });

        mb.Entity<DeliveryRecord>(e =>
        {
            e.HasKey(d => d.DeliveryId);
            e.Property(d => d.EarningsForDelivery).HasPrecision(18, 2);
            e.HasOne(d => d.Agent)
             .WithMany(a => a.Deliveries)
             .HasForeignKey(d => d.AgentId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
