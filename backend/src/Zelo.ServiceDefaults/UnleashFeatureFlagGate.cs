using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zelo.ServiceDefaults;

internal sealed class UnleashFeatureFlagGate(
    HttpClient httpClient,
    IOptions<FeatureFlagsOptions> options,
    ILogger<UnleashFeatureFlagGate> logger) : IFeatureFlagGate
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(10);
    private readonly ConcurrentDictionary<string, (bool Value, DateTimeOffset ExpiresAt)> _cache = new();
    private readonly FeatureFlagsOptions _options = options.Value;

    public async Task<bool> IsEnabledAsync(string flagName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Url) || string.IsNullOrWhiteSpace(_options.ApiToken))
            return true;

        if (_cache.TryGetValue(flagName, out var cached) && cached.ExpiresAt > DateTimeOffset.UtcNow)
            return cached.Value;

        var value = await FetchAsync(flagName, ct);
        _cache[flagName] = (value, DateTimeOffset.UtcNow.Add(CacheTtl));
        return value;
    }

    private async Task<bool> FetchAsync(string flagName, CancellationToken ct)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/admin/projects/default/features/{flagName}");
            request.Headers.TryAddWithoutValidation("Authorization", _options.ApiToken);

            using var response = await httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return true;

            var feature = await response.Content.ReadFromJsonAsync<FeatureResponse>(ct);
            var environment = feature?.Environments.FirstOrDefault(e => e.Name == _options.Environment);
            return environment?.Enabled ?? true;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Nao foi possivel consultar a flag {FlagName} no Unleash - a assumir ligada", flagName);
            return true;
        }
    }

    private sealed record FeatureResponse(FeatureEnvironment[] Environments);

    private sealed record FeatureEnvironment(string Name, bool Enabled);
}
