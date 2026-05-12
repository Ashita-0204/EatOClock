using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Notification_Service.Data;
using Notification_Service.Hubs;
using Notification_Service.Interfaces;
using Notification_Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"]));

var jwtKey = builder.Configuration["Jwt:Key"] ?? "mysecretkey1234567890mysecretkey1234567890";

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
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
    // SignalR sends JWT via query string during WebSocket upgrade
    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var token = ctx.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token) &&
                ctx.HttpContext.Request.Path.StartsWithSegments("/hubs/notifications"))
                ctx.Token = token;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// SignalR: allow the gateway origin so the WebSocket upgrade CORS check passes.
// The gateway URL is supplied via env var on Render; falls back to localhost for dev.
var gatewayOrigin = builder.Configuration["Gateway:Origin"] ?? "http://localhost:5000";
var frontendOrigin = builder.Configuration["Frontend:Origin"] ?? "http://localhost:4200";

builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins(gatewayOrigin, frontendOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationServiceImpl>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<ISmsService, SmsService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EatOClock Notification API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Trust the reverse proxy headers Render sets (X-Forwarded-For / X-Forwarded-Proto)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07")
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Migration already applied (tables exist). Inserting migration history record...");

        db.Database.ExecuteSqlRaw("""
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            SELECT '20260423232817_InitialCreate', '8.0.0'
            WHERE NOT EXISTS (
                SELECT 1 FROM "__EFMigrationsHistory"
                WHERE "MigrationId" = '20260423232817_InitialCreate'
            );
        """);
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "EatOClock Notification API v1");
});
app.UseCors("SignalRPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
