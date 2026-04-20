using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Auth_Service.Models;

namespace Auth_Service.Data;
public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Seed roles
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole("Customer") { Id = "1", NormalizedName = "CUSTOMER" },
            new IdentityRole("RestaurantOwner") { Id = "2", NormalizedName = "RESTAURANTOWNER" },
            new IdentityRole("DeliveryAgent") { Id = "3", NormalizedName = "DELIVERYAGENT" },
            new IdentityRole("Admin") { Id = "4", NormalizedName = "ADMIN" }
        );
    }
}