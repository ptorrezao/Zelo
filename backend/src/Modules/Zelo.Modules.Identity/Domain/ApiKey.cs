namespace Zelo.Modules.Identity.Domain;

/// Credencial de longa duracao para agentes/integracoes (ex.: os tools MCP
/// do Auto) - alternativa ao bearer token de sessao, que expira em 1h e
/// nao serve para um cliente que fica sempre ligado. Nunca guardamos o
/// valor em claro: so o hash e um Prefix curto para o utilizador
/// reconhecer a chave na lista (ver ApiKeyEndpointHandlers.Create).
internal sealed class ApiKey
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public required string Name { get; set; }

    /// SHA-256 do valor completo da chave, em hex minusculo - e por isto
    /// que se procura (ver ApiKeyAuthenticationHandler), nunca pelo Id.
    public required string KeyHash { get; init; }

    /// Primeiros carateres do valor da chave (depois do prefixo "zelo_"),
    /// só para o utilizador distinguir chaves na lista - nunca chega para
    /// reconstruir o valor completo.
    public required string DisplayPrefix { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastUsedAt { get; set; }

    /// Null enquanto a chave esta ativa. Nunca eliminamos a linha ao
    /// revogar - mantem-se o audit trail de quando foi criada e usada.
    public DateTimeOffset? RevokedAt { get; set; }
}
