namespace Zelo.SharedKernel;

/// Nome do scheme de autenticacao e do header usados pelas chaves de API
/// pessoais (ver Zelo.Modules.Identity.Domain.ApiKey e
/// ApiKeyAuthenticationHandler). Vive aqui, e nao no modulo Identity, para
/// outros modulos poderem pedir este scheme explicitamente (ver
/// AutoModule.MapAutoMcpEndpoints) sem violarem a fronteira de modulos -
/// nenhum modulo pode referenciar outro diretamente.
public static class ApiKeyDefaults
{
    public const string Scheme = "ApiKey";
    public const string HeaderName = "X-Api-Key";
}
