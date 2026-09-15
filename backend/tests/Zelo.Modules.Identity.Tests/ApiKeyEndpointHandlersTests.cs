using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Zelo.Modules.Identity.Endpoints;
using Zelo.Modules.Identity.Infrastructure;

namespace Zelo.Modules.Identity.Tests;

public class ApiKeyEndpointHandlersTests
{
    private static IdentityDbContext NewDb() =>
        new(new DbContextOptionsBuilder<IdentityDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ClaimsPrincipal PrincipalFor(Guid userId) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())]));

    [Fact]
    public async Task CreateApiKey_DevolveAChaveEmClaro_MasSoGuardaOHash()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();

        var result = await ApiKeyEndpointHandlers.CreateApiKey(
            new CreateApiKeyRequest("Claude"), PrincipalFor(userId), db, CancellationToken.None);

        var created = Assert.IsType<Created<CreateApiKeyResponse>>(result);
        Assert.StartsWith("zelo_", created.Value!.Key);
        Assert.Equal("Claude", created.Value.Name);

        var stored = await db.ApiKeys.SingleAsync();
        Assert.Equal(userId, stored.UserId);
        Assert.Equal(ApiKeyHasher.Hash(created.Value.Key), stored.KeyHash);
        Assert.NotEqual(created.Value.Key, stored.KeyHash);
    }

    [Fact]
    public async Task CreateApiKey_NomeVazio_DevolveBadRequest()
    {
        await using var db = NewDb();

        var result = await ApiKeyEndpointHandlers.CreateApiKey(
            new CreateApiKeyRequest(""), PrincipalFor(Guid.NewGuid()), db, CancellationToken.None);

        Assert.IsType<BadRequest<ErrorResponse>>(result);
    }

    [Fact]
    public async Task GetMyApiKeys_SoDevolveAsChavesDoUtilizador()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        await ApiKeyEndpointHandlers.CreateApiKey(new CreateApiKeyRequest("Minha chave"), PrincipalFor(userId), db, CancellationToken.None);
        await ApiKeyEndpointHandlers.CreateApiKey(new CreateApiKeyRequest("De outro utilizador"), PrincipalFor(Guid.NewGuid()), db, CancellationToken.None);

        var keys = await ApiKeyEndpointHandlers.GetMyApiKeys(PrincipalFor(userId), db, CancellationToken.None);

        Assert.Single(keys);
        Assert.Equal("Minha chave", keys[0].Name);
    }

    [Fact]
    public async Task RevokeApiKey_MarcaComoRevogada()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        var created = await ApiKeyEndpointHandlers.CreateApiKey(new CreateApiKeyRequest("Claude"), PrincipalFor(userId), db, CancellationToken.None);
        var id = ((Created<CreateApiKeyResponse>)created).Value!.Id;

        var result = await ApiKeyEndpointHandlers.RevokeApiKey(id, PrincipalFor(userId), db, CancellationToken.None);

        Assert.IsType<NoContent>(result);
        Assert.NotNull((await db.ApiKeys.FindAsync(id))!.RevokedAt);
    }

    [Fact]
    public async Task RevokeApiKey_DeOutroUtilizador_DevolveNotFound()
    {
        await using var db = NewDb();
        var owner = Guid.NewGuid();
        var created = await ApiKeyEndpointHandlers.CreateApiKey(new CreateApiKeyRequest("Claude"), PrincipalFor(owner), db, CancellationToken.None);
        var id = ((Created<CreateApiKeyResponse>)created).Value!.Id;

        var result = await ApiKeyEndpointHandlers.RevokeApiKey(id, PrincipalFor(Guid.NewGuid()), db, CancellationToken.None);

        Assert.IsType<NotFound>(result);
        Assert.Null((await db.ApiKeys.FindAsync(id))!.RevokedAt);
    }
}
