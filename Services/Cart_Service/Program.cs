using Cart_Service.Data;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Cart_Service.Interfaces;
using Cart_Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -- EF Core (Postgres) ----------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsHistoryTable("__EFMigrationsHistory", "orders")));

// -- Redis distributed cache (fallback safe) -------------------------------
var redisConn = builder.Configuration.GetConnectionString("Redis");
var useRedis = false;

if (!string.IsNullOrWhiteSpace(redisConn))
{
    try
    {
        var redisConfig = StackExchange.Redis.ConfigurationOptions.Parse(redisConn);
        redisConfig.ConnectTimeout = 2000;
        redisConfig.AbortOnConnectFail = false;

        using var testConn = StackExchange.Redis.ConnectionMultiplexer.Connect(redisConfig);
        useRedis = testConn.IsConnected;
    }
    catch
    {
        useRedis = false;
    }
}

if (useRedis)
{
    Console.WriteLine("[Cache] Redis connected - using Redis distributed cache.");
    builder.Services.AddStackExchangeRedisCache(opt => opt.Configuration = redisConn);
}
else
{
    Console.WriteLine("[Cache] Redis unavailable - using in-memory cache.");
    builder.Services.AddDistributedMemoryCache();
}

// -- JWT CONFIG ------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSection["Key"];
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new Exception("JWT Key missing");

if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new Exception("JWT Issuer missing");

if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new Exception("JWT Audience missing");

Console.WriteLine($"JWT KEY LOADED: {jwtKey}");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;


    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        ),

        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        ValidateAudience = true,
        ValidAudience = jwtAudience,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,

        RequireSignedTokens = true
    };
});

builder.Services.AddAuthorization();

// -- SERVICES --------------------------------------------------------------
builder.Services.AddScoped<ICartService, CartServiceImpl>();
builder.Services.AddControllers();

// -- SWAGGER ---------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EatOClock Cart API",
        Version = "v1"
    });

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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// -- AUTO MIGRATION --------------------------------------------------------
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Console.WriteLine("Applying Cart migrations...");
        await db.Database.MigrateAsync();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error during Cart migrations: {ex.Message}");
}

// -- MIDDLEWARE -----------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "Cart API V1");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () =>
    Results.Ok(new
    {
        status = "Healthy",
        service = "Cart-Service",
        time = DateTime.UtcNow
    }));

app.MapGet("/api/v1/cart/health", () => Results.Ok(new { status = "Healthy", service = "Cart-Service", version = "v1", time = DateTime.UtcNow }));

app.Run();