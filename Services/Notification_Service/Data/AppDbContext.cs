using Microsoft.EntityFrameworkCore;
using Notification_Service.Models;

namespace Notification_Service.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.NotificationId);
            e.Property(x => x.RecipientId).IsRequired();
            e.Property(x => x.Title).IsRequired();
            e.Property(x => x.Message).IsRequired();
            e.HasIndex(x => x.RecipientId);
        });
    }
}
