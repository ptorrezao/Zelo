using Zelo.Modules.Identity.Domain;

namespace Zelo.Modules.Identity.Endpoints;

internal sealed record CreateApiKeyRequest(string Name);

/// So aparece na resposta de Create - Key nunca e devolvido depois disso
/// (so guardamos o hash, ver ApiKey.KeyHash).
internal sealed record CreateApiKeyResponse(Guid Id, string Name, string Key, string DisplayPrefix, DateTimeOffset CreatedAt)
{
    public static CreateApiKeyResponse From(ApiKey k, string rawKey) => new(k.Id, k.Name, rawKey, k.DisplayPrefix, k.CreatedAt);
}

internal sealed record ApiKeyResponse(
    Guid Id, string Name, string DisplayPrefix, DateTimeOffset CreatedAt, DateTimeOffset? LastUsedAt, DateTimeOffset? RevokedAt)
{
    public static ApiKeyResponse From(ApiKey k) => new(k.Id, k.Name, k.DisplayPrefix, k.CreatedAt, k.LastUsedAt, k.RevokedAt);
}
