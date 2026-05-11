using Microsoft.EntityFrameworkCore;
using Restaurant_Service.Models;

namespace Restaurant_Service.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Restaurant> Restaurants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("restaurants");

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Cuisine);
            entity.HasIndex(e => e.IsApproved);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.Latitude, e.Longitude });
        });
    }
}
