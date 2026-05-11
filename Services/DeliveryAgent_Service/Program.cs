using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DeliveryAgent_Service.Data;
using DeliveryAgent_Service.Hubs;
using DeliveryAgent_Service.Interfaces;
using DeliveryAgent_Service.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// -- Database ------------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"],
        x => x.MigrationsHistoryTable("__EFMigrationsHistory", "delivery")));

// -- JWT Auth ------------------------------------------------------------------
var jwtKey = builder.Configuration["Jwt:Key"] ?? "mysecretkey1234567890mysecretkey1234567890";

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidateAudience         = true,
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };

    // Allow SignalR to read token from query string
    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var token = ctx.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs/location"))
                ctx.Token = token;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// -- SignalR -------------------------------------------------------------------
builder.Services.AddSignalR();

// -- App Services --------------------------------------------------------------
builder.Services.AddScoped<IAgentService, AgentServiceImpl>();
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// -- Swagger -------------------------------------------------------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EatOClock DeliveryAgent API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.Http,
        Scheme      = "Bearer",
        BearerFormat = "JWT",
        In          = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        },
        Array.Empty<string>()
    }});
});

var app = builder.Build();

// -- Migrate on startup --------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EatOClock DeliveryAgent API v1");
    c.RoutePrefix = string.Empty;
});

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", time = DateTime.UtcNow }));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<LocationHub>("/hubs/location");

app.Run();
