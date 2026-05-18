using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;

namespace Logging;

public static class LoggingExtensions
{
    /// <summary>
    /// Configures Serilog from appsettings + console output. For use with IHostBuilder.
    /// </summary>
    public static IHostBuilder UseCustomSerilog(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, services, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration)
                  .ReadFrom.Services(services)
                  .Enrich.FromLogContext()
                  .Enrich.WithProperty("Application", context.Configuration["Serilog:Properties:Application"] ?? "EatOClock")
                  .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");
        });
        return hostBuilder;
    }

    /// <summary>
    /// Configures Serilog for WebApplicationBuilder, add service specific metadata
    /// </summary>
    public static WebApplicationBuilder UseCustomSerilog(this WebApplicationBuilder builder, string serviceName)
    {
        builder.Host.UseSerilog((context, services, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration)
                  .ReadFrom.Services(services)
                  .Enrich.FromLogContext()
                  .Enrich.WithProperty("Service", serviceName)
                  .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");
        });
        return builder;
    }

    /// <summary>
    /// Bootstrap logger for use before host is built (catches startup errors).
    /// </summary>
    public static void CreateBootstrapLogger(string serviceName)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.WithProperty("Service", serviceName)
            .WriteTo.Console()
            .CreateBootstrapLogger();
    }
}

