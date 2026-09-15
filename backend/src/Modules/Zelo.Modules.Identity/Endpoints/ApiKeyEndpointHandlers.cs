using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Zelo.Modules.Identity.Domain;
using Zelo.Modules.Identity.Infrastructure;

namespace Zelo.Modules.Identity.Endpoints;

/// Chaves de API pessoais - credencial de longa duracao para agentes e
/// integracoes (ex.: os tools MCP do Auto, ver /mcp/auto), alternativa ao
/// bearer token de sessao normal (que expira em 1h). Geridas so pela
/// sessao normal do utilizador (RequireAuthorization default, sem
/// AuthenticationSchemes explicito) - uma chave nunca serve para gerir
/// outras chaves.
internal static class ApiKeyEndpointHandlers
{
    public static async Task<List<ApiKeyResponse>> GetMyApiKeys(ClaimsPrincipal caller, IdentityDbContext db, CancellationToken ct)
    {
        var userId = RequireUserId(caller);
        return await db.ApiKeys
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => ApiKeyResponse.From(k))
            .ToListAsync(ct);
    }

    public static async Task<IResult> CreateApiKey(
        CreateApiKeyRequest request, ClaimsPrincipal caller, IdentityDbContext db, CancellationToken ct)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 200)
            return Results.BadRequest(new ErrorResponse("Nome inválido (1 a 200 caracteres)."));

        var userId = RequireUserId(caller);
        var rawKey = ApiKeyHasher.GenerateRawKey();

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            KeyHash = ApiKeyHasher.Hash(rawKey),
            DisplayPrefix = ApiKeyHasher.DisplayPrefix(rawKey),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.ApiKeys.Add(apiKey);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/api-keys/{apiKey.Id}", CreateApiKeyResponse.From(apiKey, rawKey));
    }

    public static async Task<IResult> RevokeApiKey(Guid id, ClaimsPrincipal caller, IdentityDbContext db, CancellationToken ct)
    {
        var userId = RequireUserId(caller);
        var apiKey = await db.ApiKeys.FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId, ct);
        if (apiKey is null)
            return Results.NotFound();

        apiKey.RevokedAt ??= DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static Guid RequireUserId(ClaimsPrincipal caller) =>
        Guid.TryParse(caller.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : throw new InvalidOperationException("Endpoint exige RequireAuthorization com NameIdentifier - falta na pipeline.");
}
