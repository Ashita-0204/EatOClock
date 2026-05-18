using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Auth_Service.Models;

namespace Auth_Service.Data;
public class AppDbContext : IdentityDbContext<User>
{
    //Identity Db Context == special db context , tbales like
    //roles,users,tokens etc n user here is from models(custom models)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);// so that relationship dont break
        builder.HasDefaultSchema("auth_custom"); //supabase svhema
        // Roles are seeded at runtime in Program.cs via RoleManager
    }
}