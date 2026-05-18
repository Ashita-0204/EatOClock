using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;

namespace HealthChecks;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Registers a basic liveness health check for any microservice.
    /// </summary>
    public static IHealthChecksBuilder AddServiceHealthChecks(this IServiceCollection services)
    {
        return services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("Service is running"), tags: new[] { "live" });
    }

    /// <summary>
    /// Maps /health with a JSON response.
    /// IEndpoint -- used for defining and registering HTTP endpoints
    /// </summary>
    public static IEndpointRouteBuilder MapServiceHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    timestamp = DateTime.UtcNow
                }));
            }
        });
        return endpoints;
    }
}
