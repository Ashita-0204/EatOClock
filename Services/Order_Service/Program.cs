using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Order_Service.Data;
using Order_Service.Interfaces;
using Order_Service.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsHistoryTable("__EFMigrationsHistory", "orders")));
        
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"] ?? throw new Exception("JWT Key missing");
var issuer = jwt["Issuer"] ?? throw new Exception("JWT Issuer missing");
var audience = jwt["Audience"] ?? throw new Exception("JWT Audience missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = false;
        opt.SaveToken = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true, ValidIssuer = issuer,
            ValidateAudience = true, ValidAudience = audience,
            ValidateLifetime = true, ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// URLs are supplied via env vars on Render:
//   Services__NotificationService=https://eatoclock-notification.onrender.com
//   Services__RestaurantService=https://eatoclock-restaurant.onrender.com
// Locally they fall back to the docker-compose hostnames.
var notificationUrl = builder.Configuration["Services:NotificationService"]
                      ?? "http://notification-service:8080";
var restaurantUrl   = builder.Configuration["Services:RestaurantService"]
                      ?? "http://restaurant-service:8080";

builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri(notificationUrl);
});

builder.Services.AddHttpClient("RestaurantService", client =>
{
    client.BaseAddress = new Uri(restaurantUrl);
});

builder.Services.AddScoped<IOrderService, OrderServiceImpl>();
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EatOClock Order API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", Type = SecuritySchemeType.Http,
        Scheme = "Bearer", BearerFormat = "JWT", In = ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
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

try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Console.WriteLine("Applying Order migrations...");
        await db.Database.MigrateAsync();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error during Order migrations: {ex.Message}");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "Order API V1");
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "Order-Service",
    time = DateTime.UtcNow
}));

app.MapGet("/api/v1/orders/health", () => Results.Ok(new { status = "Healthy", service = "Order-Service", version = "v1", time = DateTime.UtcNow }));

app.Run();
