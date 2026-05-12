using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Auth_Service.Data;
using Auth_Service.Models;
using Auth_Service.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Auth_Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"],
        x => x.MigrationsHistoryTable("__EFMigrationsHistory", "auth_custom")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "mysecretkey1234567890mysecretkey1234567890";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ── CORS ─────────────────────────────────────────────────────────────────────
// Allow calls from the Angular dev server and from the API gateway.
// Extend AllowedOrigins in appsettings for production.
var corsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
// ─────────────────────────────────────────────────────────────────────────────

builder.Services.AddScoped<IAuthService, AuthServiceImpl>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EatOClock Auth API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Run migrations + seed roles
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Console.WriteLine("Applying migrations...");
        await dbContext.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "Customer", "RestaurantOwner", "DeliveryAgent", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed default admin if none exists
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        const string adminEmail = "admin@eatoclock.com";
        const string adminPassword = "Admin@123456";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            if (string.IsNullOrEmpty(adminUser.SecurityStamp))
            {
                await userManager.UpdateSecurityStampAsync(adminUser);
            }

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            await userManager.ResetPasswordAsync(adminUser, resetToken, adminPassword);
        }

        // Fix and normalize all existing users to ensure they can login
        // Commented out to prevent massive startup delays which cause Render 502 errors
        /*
        Console.WriteLine("Starting user normalization and security stamp fix...");
        var allUsers = await userManager.Users.ToListAsync();
        int fixedCount = 0;
        foreach (var user in allUsers)
        {
            bool changed = false;
            if (string.IsNullOrEmpty(user.SecurityStamp))
            {
                await userManager.UpdateSecurityStampAsync(user);
                changed = true;
            }
            
            var normalizedEmail = userManager.NormalizeEmail(user.Email!);
            var normalizedName = userManager.NormalizeName(user.UserName!);
            
            if (user.NormalizedEmail != normalizedEmail || user.NormalizedUserName != normalizedName)
            {
                user.NormalizedEmail = normalizedEmail;
                user.NormalizedUserName = normalizedName;
                changed = true;
            }

            if (changed)
            {
                await userManager.UpdateAsync(user);
                fixedCount++;
            }
        }
        Console.WriteLine($"User normalization complete. Fixed {fixedCount} users.");
        */

    }
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred during startup migration/seeding: {ex.Message}");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "EatOClock Auth API v1");
});

// CORS must come before UseAuthentication/UseAuthorization
app.UseCors("FrontendPolicy");

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", time = DateTime.UtcNow }));
app.MapGet("/api/v1/auth/health", () => Results.Ok(new { status = "Healthy", service = "Auth-Service", version = "v1", time = DateTime.UtcNow }));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();