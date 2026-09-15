using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zelo.SharedKernel;

namespace Zelo.Modules.Identity.Infrastructure;

/// Autentica pedidos com uma chave de API pessoal (ver ApiKey), no header
/// X-Api-Key (ApiKeyDefaults.HeaderName). So usado onde explicitamente
/// pedido via RequireAuthorization(p => p.AddAuthenticationSchemes(...))
/// - ver AutoModule.MapAutoMcpEndpoints. A sessao normal do browser
/// continua no scheme por omissao (Identity.Bearer), sem qualquer
/// interferencia desta classe.
internal sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IdentityDbContext db)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyDefaults.HeaderName, out var headerValues))
            return AuthenticateResult.NoResult();

        var rawKey = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(rawKey))
            return AuthenticateResult.NoResult();

        var hash = ApiKeyHasher.Hash(rawKey);
        var apiKey = await db.ApiKeys.FirstOrDefaultAsync(k => k.KeyHash == hash, Context.RequestAborted);
        if (apiKey is null || apiKey.RevokedAt is not null)
            return AuthenticateResult.Fail("Chave de API inválida ou revogada.");

        // Com await, mesmo sendo so informativo - o DbContext e scoped ao
        // pedido e e descartado no fim deste; disparar sem esperar arriscava
        // escrever num contexto ja em dispose.
        apiKey.LastUsedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(Context.RequestAborted);

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, apiKey.UserId.ToString())],
            ApiKeyDefaults.Scheme);
        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, ApiKeyDefaults.Scheme));
    }
}
