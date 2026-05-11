using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "EatOClock.Gateway")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting EatOClock API Gateway");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithProperty("Service", "EatOClock.Gateway")
              .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

    // ── CORS ─────────────────────────────────────────────────────────────────
    // Allows the Angular dev server (and any configured production origin) to
    // call the gateway.  Adjust the origin list for production deployments.
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
    // ─────────────────────────────────────────────────────────────────────────

    // JWT Auth — gateway validates tokens before proxying
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "mysecretkey1234567890mysecretkey1234567890";
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

    builder.Services.AddAuthorization();

    // Health checks for all downstream services
    builder.Services.AddHealthChecks()
        .AddUrlGroup(new Uri(builder.Configuration["Services:Auth"]          + "/health"), name: "auth-service",              tags: new[] { "services" })
        .AddUrlGroup(new Uri(builder.Configuration["Services:Restaurant"]    + "/health"), name: "restaurant-service",        tags: new[] { "services" })
        .AddUrlGroup(new Uri(builder.Configuration["Services:Menu"]          + "/health"), name: "menu-service",              tags: new[] { "services" })
        .AddUrlGroup(new Uri(builder.Configuration["Services:Cart"]          + "/health"), name: "cart-service",              tags: new[] { "services" })
        .AddUrlGroup(new Uri(builder.Configuration["Services:Order"]         + "/health"), name: "order-service",             tags: new[] { "services" })
        .AddUrlGroup(new Uri(builder.Configuration["Services:Payment"]       + "/health"), name: "payment-service",           tags: new[] { "services" })
        .AddUrlGroup(new Uri(builder.Configuration["Services:Notification"]  + "/health"), name: "notification-service",      tags: new[] { "services" })
        .AddUrlGroup(new Uri(builder.Configuration["Services:DeliveryAgent"] + "/health"), name: "delivery-agent-service",    tags: new[] { "services" })
        .AddUrlGroup(new Uri(builder.Configuration["Services:Review"]        + "/health"), name: "review-service",            tags: new[] { "services" });

    // YARP Reverse Proxy
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    var app = builder.Build();

    // ── Middleware order matters ──────────────────────────────────────────────
    // CORS must come BEFORE authentication/authorization and the proxy
    app.UseCors("FrontendPolicy");

    // Request/response logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            var userId = httpContext.User?.FindFirst("sub")?.Value
                      ?? httpContext.User?.FindFirst("nameid")?.Value;
            if (userId != null) diagnosticContext.Set("UserId", userId);
        };
    });

    app.UseAuthentication();
    app.UseAuthorization();

    // Gateway self health
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => false,
        ResponseWriter = async (ctx, report) =>
        {
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                status = "Healthy",
                service = "EatOClock.Gateway",
                timestamp = DateTime.UtcNow
            }));
        }
    });

    // All downstream services health
    app.MapHealthChecks("/health/services", new HealthCheckOptions
    {
        Predicate = hc => hc.Tags.Contains("services"),
        ResponseWriter = async (ctx, report) =>
        {
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                services = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    durationMs = e.Value.Duration.TotalMilliseconds,
                    error = e.Value.Exception?.Message
                })
            }));
        }
    });

    app.Use(async (context, next) =>
    {
        Log.Information("Incoming Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        await next();
        Log.Information("Response: {StatusCode}", context.Response.StatusCode);
    });

    app.MapReverseProxy();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}