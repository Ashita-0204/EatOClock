using Microsoft.EntityFrameworkCore;
using Review_Service.Models;

namespace Review_Service.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("analytics");
        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(r => r.ReviewId);
            e.HasIndex(r => r.OrderId).IsUnique();   // UC-52: one review per order
            e.Property(r => r.CustomerId).IsRequired();
        });
    }
}
