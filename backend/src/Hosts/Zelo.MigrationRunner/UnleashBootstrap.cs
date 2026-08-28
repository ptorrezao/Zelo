using System.Net;
using System.Net.Http.Json;

namespace Zelo.MigrationRunner;

/// Garante que as feature flags que a app precisa existem no Unleash e
/// estao ligadas - sem isto, a primeira vez que alguem sobe a stack de
/// raiz as flags nao existem e o frontend cai no fallback (fail-open,
/// ver useFeatureFlags no frontend). So o Admin API cria/ativa flags -
/// nao ha SDK client aqui, e so isto que precisamos.
internal static class UnleashBootstrap
{
    private static readonly string[] Environments = ["development", "production"];

    public static readonly (string Name, string Description)[] RequiredFlags =
    [
        ("auto-app-enabled", "Mostra a app Auto na navegacao"),
        ("inventory-app-enabled", "Mostra a app Inventario na navegacao"),
    ];

    public static async Task RunAsync(
        string baseUrl, string apiToken, HttpMessageHandler? handler = null, CancellationToken ct = default)
    {
        using var client = handler is null ? new HttpClient() : new HttpClient(handler);
        client.BaseAddress = new Uri(baseUrl);
        // TryAddWithoutValidation: o token do Unleash ("*:*.segredo") nao e
        // um valor valido de Authorization RFC 7230 (nao tem esquema), o
        // .Add() normal rejeita-o.
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiToken);

        await WaitUntilReadyAsync(client, ct);

        foreach (var (name, description) in RequiredFlags)
        {
            await EnsureFlagExistsAsync(client, name, description, ct);
            foreach (var environment in Environments)
                await EnsureFlagEnabledAsync(client, name, environment, ct);
        }
    }

    private static async Task WaitUntilReadyAsync(HttpClient client, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                var response = await client.GetAsync("/api/admin/projects", ct);
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch (HttpRequestException)
            {
                // Unleash ainda nao aceita ligacoes - tenta outra vez.
            }

            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        throw new InvalidOperationException("Unleash nao ficou pronto a tempo do bootstrap de feature flags.");
    }

    private static async Task EnsureFlagExistsAsync(HttpClient client, string name, string description, CancellationToken ct)
    {
        var existing = await client.GetAsync($"/api/admin/projects/default/features/{name}", ct);
        if (existing.StatusCode != HttpStatusCode.NotFound)
            return;

        var response = await client.PostAsJsonAsync(
            "/api/admin/projects/default/features",
            new { name, description, type = "release" },
            ct);
        response.EnsureSuccessStatusCode();
    }

    private static async Task EnsureFlagEnabledAsync(HttpClient client, string name, string environment, CancellationToken ct)
    {
        var response = await client.PostAsync(
            $"/api/admin/projects/default/features/{name}/environments/{environment}/on",
            content: null,
            ct);
        response.EnsureSuccessStatusCode();
    }
}
