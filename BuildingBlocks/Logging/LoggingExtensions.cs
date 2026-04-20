using Microsoft.Extensions.Hosting;
using Serilog;
namespace Logging;

public static class LoggingExtensions
{
    public static IHostBuilder UseCustomSerialog(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration).Enrich.WithProperty("Application", "QuickBite")
                  .WriteTo.Console();
        });
        return hostBuilder;
    }
}
