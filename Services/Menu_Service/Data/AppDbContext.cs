using Microsoft.EntityFrameworkCore;
using Menu_Service.Models;

namespace Menu_Service.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MenuCategory> Categories { get; set; }
    public DbSet<MenuItem> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("restaurants");

        modelBuilder.Entity<MenuCategory>(e =>
        {
            e.ToTable("Categories");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.RestaurantId);
            e.HasIndex(x => x.IsActive);
            e.HasMany(x => x.Items)
             .WithOne(i => i.Category)
             .HasForeignKey(i => i.CategoryId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MenuItem>(e =>
        {
            e.ToTable("Items");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.RestaurantId);
            e.HasIndex(x => x.CategoryId);
            e.HasIndex(x => x.IsAvailable);
        });
    }
}
