using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zelo.MigrationRunner;

/// Garante que o cluster Garage (1 no) tem layout, bucket e chave prontos
/// a usar. Sem isto o modulo Auto nao consegue gerar URLs de upload -
/// antes disto era um passo manual documentado em comentario no
/// docker-compose.yml.
internal static class GarageBootstrap
{
    public static async Task RunAsync(
        string adminUrl,
        string adminToken,
        string bucketName,
        string accessKeyId,
        string secretAccessKey,
        string keyName,
        HttpMessageHandler? handler = null,
        CancellationToken ct = default)
    {
        using var client = handler is null ? new HttpClient() : new HttpClient(handler);
        client.BaseAddress = new Uri(adminUrl);
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {adminToken}");

        var nodeId = await WaitForNodeIdAsync(client, ct);
        await EnsureLayoutAsync(client, nodeId, ct);
        var bucketId = await EnsureBucketAsync(client, bucketName, ct);
        await EnsureKeyImportedAsync(client, accessKeyId, secretAccessKey, keyName, ct);
        await EnsureBucketAccessAsync(client, bucketId, accessKeyId, ct);
    }

    private static async Task<string> WaitForNodeIdAsync(HttpClient client, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                var status = await client.GetFromJsonAsync<StatusResponse>("/v1/status", ct);
                var nodeId = status?.Nodes.FirstOrDefault(n => n.IsUp)?.Id;
                if (nodeId is not null)
                    return nodeId;
            }
            catch (HttpRequestException)
            {
                // Garage ainda nao aceita ligacoes - tenta outra vez.
            }

            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        throw new InvalidOperationException("Garage nao ficou pronto a tempo do bootstrap.");
    }

    private static async Task EnsureLayoutAsync(HttpClient client, string nodeId, CancellationToken ct)
    {
        var layout = await client.GetFromJsonAsync<LayoutResponse>("/v1/layout", ct)
            ?? throw new InvalidOperationException("Nao foi possivel ler o layout do Garage.");

        if (layout.Roles.Any(r => r.Id == nodeId))
            return; // ja tem role atribuida - nada a fazer

        var stageBody = JsonSerializer.SerializeToUtf8Bytes(new[]
        {
            new { id = nodeId, zone = "dc1", capacity = 1_000_000_000L, tags = Array.Empty<string>() },
        });
        using var stageContent = new ByteArrayContent(stageBody);
        stageContent.Headers.ContentType = new("application/json");
        var stageResponse = await client.PostAsync("/v1/layout", stageContent, ct);
        stageResponse.EnsureSuccessStatusCode();

        var applyResponse = await client.PostAsJsonAsync("/v1/layout/apply", new { version = layout.Version + 1 }, ct);
        applyResponse.EnsureSuccessStatusCode();
    }

    private static async Task<string> EnsureBucketAsync(HttpClient client, string bucketName, CancellationToken ct)
    {
        var existing = await client.GetAsync($"/v1/bucket?globalAlias={Uri.EscapeDataString(bucketName)}", ct);
        if (existing.StatusCode == HttpStatusCode.OK)
        {
            var bucket = await existing.Content.ReadFromJsonAsync<BucketResponse>(ct);
            return bucket!.Id;
        }

        var createResponse = await client.PostAsJsonAsync("/v1/bucket", new { globalAlias = bucketName }, ct);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<BucketResponse>(ct);
        return created!.Id;
    }

    private static async Task EnsureKeyImportedAsync(
        HttpClient client, string accessKeyId, string secretAccessKey, string keyName, CancellationToken ct)
    {
        var existing = await client.GetAsync($"/v1/key?id={Uri.EscapeDataString(accessKeyId)}", ct);
        if (existing.StatusCode == HttpStatusCode.OK)
            return; // a chave (com este segredo) so pode ser vista uma vez, no import

        var importResponse = await client.PostAsJsonAsync(
            "/v1/key/import",
            new { accessKeyId, secretAccessKey, name = keyName },
            ct);
        importResponse.EnsureSuccessStatusCode();
    }

    private static async Task EnsureBucketAccessAsync(HttpClient client, string bucketId, string accessKeyId, CancellationToken ct)
    {
        // Idempotente por natureza - conceder outra vez os mesmos
        // acessos nao tem efeito secundario.
        var response = await client.PostAsJsonAsync(
            "/v1/bucket/allow",
            new { bucketId, accessKeyId, permissions = new { read = true, write = true, owner = false } },
            ct);
        response.EnsureSuccessStatusCode();
    }

    private sealed record StatusResponse([property: JsonPropertyName("nodes")] StatusNode[] Nodes);

    private sealed record StatusNode(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("isUp")] bool IsUp);

    private sealed record LayoutResponse(
        [property: JsonPropertyName("version")] int Version,
        [property: JsonPropertyName("roles")] LayoutRole[] Roles);

    private sealed record LayoutRole([property: JsonPropertyName("id")] string Id);

    private sealed record BucketResponse([property: JsonPropertyName("id")] string Id);
}
