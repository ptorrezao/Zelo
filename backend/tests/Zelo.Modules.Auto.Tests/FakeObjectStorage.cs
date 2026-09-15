using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto.Tests;

internal sealed class FakeObjectStorage : IObjectStorage
{
    public Dictionary<string, (byte[] Content, string ContentType)> Uploaded { get; } = [];

    public (Uri UploadUrl, DateTimeOffset ExpiresAt) CreateUploadUrl(string objectKey, string contentType) =>
        (new Uri($"http://storage.local/{objectKey}"), DateTimeOffset.UtcNow.AddMinutes(15));

    public Task UploadAsync(string objectKey, byte[] content, string contentType, CancellationToken ct = default)
    {
        Uploaded[objectKey] = (content, contentType);
        return Task.CompletedTask;
    }

    public Uri CreateReadUrl(string objectKey, TimeSpan validFor) =>
        new($"http://storage.local/{objectKey}?read=1");
}
