using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace Zelo.Modules.Auto.Infrastructure;

/// Garage (S3-compatible, self-hosted) atras da mesma interface que
/// qualquer outro backend S3 usaria - trocar de Garage exige so trocar
/// esta classe, nunca os chamadores.
internal sealed class GarageObjectStorage : IObjectStorage
{
    private readonly AmazonS3Client _client;
    private readonly StorageOptions _options;

    public GarageObjectStorage(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        _client = new AmazonS3Client(
            _options.AccessKey,
            _options.SecretKey,
            new AmazonS3Config
            {
                ServiceURL = _options.Endpoint,
                ForcePathStyle = true,
                AuthenticationRegion = _options.Region,
                // O SDK assume https por omissao independentemente do
                // esquema em ServiceURL - o Garage aqui nao tem TLS.
                UseHttp = _options.Endpoint.StartsWith("http://", StringComparison.Ordinal),
            });
    }

    public (Uri UploadUrl, DateTimeOffset ExpiresAt) CreateUploadUrl(string objectKey, string contentType)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);

        var url = _client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = expiresAt.UtcDateTime,
            ContentType = contentType,
        });

        // O SDK gera sempre "https://" aqui, mesmo com UseHttp=true e um
        // ServiceURL "http://" - nao ha TLS no Garage local. O esquema
        // nao entra na assinatura SigV4 (so host+path+query), por isso
        // trocar depois e seguro e nao invalida a URL.
        if (_options.Endpoint.StartsWith("http://", StringComparison.Ordinal) && url.StartsWith("https://", StringComparison.Ordinal))
        {
            url = "http://" + url["https://".Length..];
        }

        return (new Uri(url), expiresAt);
    }
}
