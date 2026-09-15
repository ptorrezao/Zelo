# Fix: bloqueio de "importar de si mesmo" dispara entre ambientes diferentes

> **Estado: implementado** — PreviewImport agora compara o host do ambiente remoto resolvido com o do pedido atual (AutoEndpointHandlers.cs); testes adicionados/atualizados em AutoEndpointHandlersTests.cs.

## Sintoma

Ao importar veículos de outro ambiente (ex.: produção → dev local),
usando o mesmo email de login nos dois lados (situação normal — é a
mesma pessoa), a API responde sempre:

> "Não pode importar veículos de si mesmo — indique as credenciais de
> outro utilizador."

mesmo os dois ambientes sendo bases de dados completamente distintas.

## Causa raiz

[AutoEndpointHandlers.cs:150-157](backend/src/Modules/Zelo.Modules.Auto/Endpoints/AutoEndpointHandlers.cs#L150-L157),
em `PreviewImport`:

```csharp
var callerEmail = caller.FindFirstValue(ClaimTypes.Email) ?? caller.FindFirstValue(ClaimTypes.Name);
if (!string.IsNullOrEmpty(callerEmail) && string.Equals(callerEmail, request.Email, StringComparison.OrdinalIgnoreCase))
    return Results.BadRequest(new { error = "Não pode importar veículos de si mesmo — indique as credenciais de outro utilizador." });
```

O bloqueio compara **só o email**, nunca o ambiente/host. O comentário
que o justifica assume implicitamente que "mesmo email = mesmo
ambiente = no-op" — verdade só quando as duas contas vivem na mesma
instância. A funcionalidade inteira ("importação entre ambientes") é
sobre ligar a **outra** instância desta app; usar o mesmo email do
outro lado é o caso normal, não uma coincidência suspeita.

## Correção proposta

Só bloquear quando o ambiente remoto resolvido (`apiBaseUrl`, depois de
`ResolveApiBaseAsync`) é o **mesmo host** que está a servir este
pedido — nesse caso sim, é garantidamente um no-op. Passa a precisar do
`HttpContext` do pedido atual (minimal API injeta-o automaticamente
como parâmetro do handler).

```csharp
public static async Task<IResult> PreviewImport(
    Guid householdId, ImportConnectRequest request, ClaimsPrincipal caller,
    HttpContext httpContext, AutoDbContext db, IImportRemoteClient remoteClient, CancellationToken ct)
{
    if (!Uri.TryCreate(request.BaseUrl, UriKind.Absolute, out var baseUrl))
        return Results.BadRequest(new { error = "URL do ambiente de origem inválido." });

    try
    {
        var apiBaseUrl = await remoteClient.ResolveApiBaseAsync(baseUrl, ct);

        // So e no-op garantido se o ambiente remoto resolvido e este
        // mesmo host (nao so o mesmo email - o mesmo utilizador tem
        // legitimamente contas com o mesmo email em ambientes diferentes,
        // e essa e precisamente a razao de existir esta funcionalidade).
        var callerEmail = caller.FindFirstValue(ClaimTypes.Email) ?? caller.FindFirstValue(ClaimTypes.Name);
        var isSameHost = string.Equals(apiBaseUrl.Authority, httpContext.Request.Host.ToUriComponent(), StringComparison.OrdinalIgnoreCase);
        if (isSameHost && !string.IsNullOrEmpty(callerEmail) && string.Equals(callerEmail, request.Email, StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new { error = "Não pode importar veículos de si mesmo — indique as credenciais de outro utilizador." });

        var token = await remoteClient.LoginAsync(apiBaseUrl, request.Email, request.Password, ct);
        // ... resto inalterado
    }
    catch (ImportRemoteException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}
```

Pontos a rever ao implementar:
- `httpContext.Request.Host` reflete o `Host` header tal como o cliente
  bateu na API — atrás de um proxy reverso (Dokploy) isto exige
  `ForwardedHeaders` já configurado (confirmar se já está, senão a
  comparação falha sempre em produção). Verificar `Program.cs`.
- A comparação continua a exigir *também* o mesmo email — mudar só o
  host preserva o comportamento antigo quando é mesmo ambiente e emails
  diferentes (continua a não bloquear, correto).
- Mover o check para depois de `ResolveApiBaseAsync` significa que já
  não bloqueia "antes de sequer tentar ligar" ao ambiente remoto (como
  o comentário original prometia) — passa a fazer sempre esse primeiro
  pedido de descoberta. Aceitável: é um único GET sem credenciais.

## Teste

Adicionar caso ao teste de `PreviewImport` (backend): mesmo email,
`BaseUrl` de um host diferente do `TestServer` → não bloqueia, segue
para login. Mesmo email, mesmo host → bloqueia como antes.
