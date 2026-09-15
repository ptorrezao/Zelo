using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.S3;
using Amazon.S3.Model;

namespace Zelo.MigrationRunner;

/// Garante que o cluster Garage (1 no) tem layout, bucket e chave prontos
/// a usar. Sem isto o modulo Auto nao consegue gerar URLs de upload -
/// antes disto era um passo manual documentado em comentario no
/// docker-compose.yml.
///
/// Mistura API admin v1 e v2 de proposito: o Garage 2.0.0 removeu
/// especificamente GetClusterStatus e UpdateClusterLayout/ApplyClusterLayout
/// da v1 ("endpoint is no longer supported"), mas manteve tudo o resto
/// (/v1/bucket, /v1/key/import, /v1/bucket/allow) a funcionar identico -
/// so se mudou o que estava mesmo partido, confirmado a testar contra um
/// Garage v2.0.0 real. A v1 continua "deprecated" nesta versao (nao
/// removida) - se um Garage futuro a tirar de vez, e so migrar o resto.
internal static class GarageBootstrap
{
    public static async Task RunAsync(
        string adminUrl,
        string adminToken,
        string s3Endpoint,
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
        await EnsureBucketCorsAsync(s3Endpoint, bucketName, accessKeyId, secretAccessKey, handler, ct);
    }

    private static async Task<string> WaitForNodeIdAsync(HttpClient client, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                var status = await client.GetFromJsonAsync<StatusResponse>("/v2/GetClusterStatus", ct);
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

        var stageResponse = await client.PostAsJsonAsync("/v2/UpdateClusterLayout", new
        {
            roles = new[] { new { id = nodeId, zone = "dc1", capacity = 1_000_000_000L, tags = Array.Empty<string>() } },
        }, ct);
        await EnsureSuccessAsync(stageResponse, ct);

        var applyResponse = await client.PostAsJsonAsync("/v2/ApplyClusterLayout", new { version = layout.Version + 1 }, ct);
        await EnsureSuccessAsync(applyResponse, ct);
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
        await EnsureSuccessAsync(createResponse, ct);
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
        await EnsureSuccessAsync(importResponse, ct);
    }

    private static async Task EnsureBucketAccessAsync(HttpClient client, string bucketId, string accessKeyId, CancellationToken ct)
    {
        // Idempotente por natureza - conceder outra vez os mesmos
        // acessos nao tem efeito secundario. "owner" (nao so
        // read/write) e necessario para PutBucketCors (ver
        // EnsureBucketCorsAsync) - confirmado pelo Garage a rejeitar com
        // "Forbidden: Operation is not allowed for this key" sem isto.
        // Continua a nao dar nenhum acesso a nivel de cluster/admin - so
        // permissoes deste bucket especifico, via S3, nunca via a API
        // admin (essa continua fechada ao bearer token separado).
        var response = await client.PostAsJsonAsync(
            "/v1/bucket/allow",
            new { bucketId, accessKeyId, permissions = new { read = true, write = true, owner = true } },
            ct);
        await EnsureSuccessAsync(response, ct);
    }

    /// Sem isto, o upload direto do browser para uma URL pre-assinada (ver
    /// IObjectStorage.CreateUploadUrl, usado por documentos de veiculo)
    /// falha silenciosamente no browser - e sempre um pedido cross-origin
    /// (frontend e Garage vivem em hosts/portas diferentes, mesmo em
    /// producao), e sem regra CORS no bucket o browser bloqueia a resposta
    /// antes do PUT sequer sair. A assinatura SigV4 do URL ja e o controlo
    /// de acesso real - permitir qualquer origem aqui nao abre nada que
    /// essa assinatura nao cubra.
    ///
    /// Usa a API S3 nativa (PutBucketCors), nao a API admin: o campo
    /// "corsRules" do admin API v2 so existe numa versao do Garage mais
    /// recente que a v2.0.0 que temos hoje (confirmado a testar contra um
    /// Garage v2.0.0 real - o pedido "sucede" mas o campo e ignorado em
    /// silencio). PutBucketCors e uma operacao S3 bem mais antiga e
    /// universal, por isso precisa de um AmazonS3Client (SigV4, mesma
    /// credencial que acabou de ser importada) em vez do HttpClient com
    /// bearer token usado no resto deste ficheiro.
    private static async Task EnsureBucketCorsAsync(
        string s3Endpoint, string bucketName, string accessKeyId, string secretAccessKey,
        HttpMessageHandler? handler, CancellationToken ct)
    {
        var config = new AmazonS3Config
        {
            ServiceURL = s3Endpoint,
            ForcePathStyle = true,
            // Sem isto, o SDK assume "us-east-1" e o Garage rejeita com
            // "Authorization header malformed" - tem de bater com
            // s3_region no garage.toml (ver GarageObjectStorage, mesmo
            // problema ja resolvido la).
            AuthenticationRegion = "garage",
            UseHttp = s3Endpoint.StartsWith("http://", StringComparison.Ordinal),
        };
        // So em testes - deixa o RoutingFakeHttpMessageHandler dos outros
        // passos tambem intercetar este cliente S3, em vez de bater na
        // rede.
        if (handler is not null)
            config.HttpClientFactory = new FixedHandlerHttpClientFactory(handler);

        using var s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, config);

        await s3Client.PutCORSConfigurationAsync(new PutCORSConfigurationRequest
        {
            BucketName = bucketName,
            Configuration = new CORSConfiguration
            {
                Rules =
                [
                    new CORSRule { AllowedMethods = ["GET", "PUT"], AllowedOrigins = ["*"], AllowedHeaders = ["*"] },
                ],
            },
        }, ct);
    }

    /// EnsureSuccessStatusCode() por si so nao inclui o corpo da resposta -
    /// para uma API de admin que devolve o motivo do erro em JSON, perder
    /// isso torna um 400 impossivel de diagnosticar a partir dos logs.
    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(ct);
        throw new InvalidOperationException(
            $"Garage respondeu {(int)response.StatusCode} {response.StatusCode} em {response.RequestMessage?.RequestUri}: {body}");
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

    /// So para testes - AmazonS3Client nao aceita um HttpMessageHandler
    /// diretamente como o HttpClient normal, so esta fabrica.
    private sealed class FixedHandlerHttpClientFactory(HttpMessageHandler handler) : Amazon.Runtime.HttpClientFactory
    {
        public override HttpClient CreateHttpClient(Amazon.Runtime.IClientConfig clientConfig) => new(handler, disposeHandler: false);
    }
}
