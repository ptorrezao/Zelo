using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace Zelo.Modules.Auto.Infrastructure;

/// Garage (S3-compatible, self-hosted) atras da mesma interface que
/// qualquer outro backend S3 usaria - trocar de Garage exige so trocar
/// esta classe, nunca os chamadores.
///
/// Dois clientes S3, nao um: CreateUploadUrl/CreateReadUrl geram URLs que o
/// *browser* vai buscar diretamente, por isso tem de usar o host publico
/// (Endpoint). UploadAsync e um pedido que o proprio servidor faz - em dev,
/// dentro de um container, "localhost" e o proprio container, nao o Garage
/// (ver StorageOptions.InternalEndpoint) - por isso usa um cliente separado.
internal sealed class GarageObjectStorage : IObjectStorage
{
    private readonly AmazonS3Client _publicClient;
    private readonly AmazonS3Client _internalClient;
    private readonly StorageOptions _options;

    public GarageObjectStorage(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        _publicClient = CreateClient(_options.Endpoint);
        _internalClient = CreateClient(_options.InternalEndpoint ?? _options.Endpoint);
    }

    private AmazonS3Client CreateClient(string endpoint) => new(
        _options.AccessKey,
        _options.SecretKey,
        new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = _options.Region,
            // O SDK assume https por omissao independentemente do esquema
            // em ServiceURL - o Garage aqui nao tem TLS.
            UseHttp = endpoint.StartsWith("http://", StringComparison.Ordinal),
        });

    public (Uri UploadUrl, DateTimeOffset ExpiresAt) CreateUploadUrl(string objectKey, string contentType)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        var url = PreSign(_publicClient, _options.Endpoint, objectKey, HttpVerb.PUT, expiresAt, contentType);
        return (new Uri(url), expiresAt);
    }

    public async Task UploadAsync(string objectKey, byte[] content, string contentType, CancellationToken ct = default)
    {
        using var stream = new MemoryStream(content);
        await _internalClient.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            InputStream = stream,
            ContentType = contentType,
            AutoCloseStream = false,
            // O SDK assina o payload por omissao com streaming SigV4
            // (header "STREAMING-..." + trailer chunked) - o Garage nao
            // sabe interpretar isso e rejeita com "Invalid content sha256
            // hash". Desligar so o chunk encoding mantem a assinatura
            // normal (SHA256 do corpo inteiro, calculado a partida - o
            // conteudo ja esta todo em memoria) sem o streaming - ao
            // contrario de DisablePayloadSigning, nao exige HTTPS (o
            // Garage local nao tem TLS).
            UseChunkEncoding = false,
        }, ct);
    }

    public Uri CreateReadUrl(string objectKey, TimeSpan validFor) =>
        new(PreSign(_publicClient, _options.Endpoint, objectKey, HttpVerb.GET, DateTimeOffset.UtcNow.Add(validFor), contentType: null));

    private string PreSign(
        AmazonS3Client client, string endpoint, string objectKey, HttpVerb verb, DateTimeOffset expiresAt, string? contentType)
    {
        var url = client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            Verb = verb,
            Expires = expiresAt.UtcDateTime,
            ContentType = contentType,
        });

        // O SDK gera sempre "https://" aqui, mesmo com UseHttp=true e um
        // ServiceURL "http://" - nao ha TLS no Garage local. O esquema nao
        // entra na assinatura SigV4 (so host+path+query), por isso trocar
        // depois e seguro e nao invalida a URL.
        if (endpoint.StartsWith("http://", StringComparison.Ordinal) && url.StartsWith("https://", StringComparison.Ordinal))
        {
            url = "http://" + url["https://".Length..];
        }

        return url;
    }
}
