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

        return (new Uri(url), expiresAt);
    }
}
