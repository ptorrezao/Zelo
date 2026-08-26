using Microsoft.Extensions.DependencyInjection;

namespace Zelo.ServiceDefaults;

/// Telemetria, health checks e resiliencia partilhados pelos hosts.
public static class ServiceDefaults
{
    public static IServiceCollection AddZeloServiceDefaults(this IServiceCollection services)
    {
        services.AddHealthChecks();
        // TODO: OpenTelemetry (tracing, metrics, logs)
        return services;
    }
}
