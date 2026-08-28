using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Zelo.ServiceDefaults;

/// Telemetria, health checks e resiliencia partilhados pelos hosts.
public static class ServiceDefaults
{
    public static IServiceCollection AddZeloServiceDefaults(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHealthChecks();

        services.Configure<FeatureFlagsOptions>(configuration.GetSection(FeatureFlagsOptions.SectionName));
        services.AddHttpClient<IFeatureFlagGate, UnleashFeatureFlagGate>((provider, client) =>
        {
            var url = provider.GetRequiredService<IOptions<FeatureFlagsOptions>>().Value.Url;
            if (!string.IsNullOrWhiteSpace(url))
                client.BaseAddress = new Uri(url);
        });

        var otlpEndpoint = configuration["Otel:Endpoint"];
        // Sem isto a Api e o Worker caiam ambos no nome generico "zelo" e
        // ficavam indistinguiveis no Jaeger - o nome do assembly de entrada
        // (Zelo.Api / Zelo.Worker) e o que os separa.
        var serviceName = configuration["Otel:ServiceName"]
            ?? Assembly.GetEntryAssembly()?.GetName().Name
            ?? "zelo";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // Qualquer ActivitySource criado pelo codigo do Zelo
                    // (ex.: envio de email em Zelo.Modules.Identity) - sem
                    // isto o OpenTelemetry ignora spans manuais por omissao.
                    .AddSource("Zelo.*");

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(otlpEndpoint));
                }
            });

        return services;
    }
}
