using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto.Tests;

internal sealed class FakeObjectStorage : IObjectStorage
{
    public (Uri UploadUrl, DateTimeOffset ExpiresAt) CreateUploadUrl(string objectKey, string contentType) =>
        (new Uri($"http://storage.local/{objectKey}"), DateTimeOffset.UtcNow.AddMinutes(15));
}
