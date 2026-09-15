using System.Security.Cryptography;
using System.Text;

namespace Zelo.Modules.Identity.Infrastructure;

/// Formato e hashing partilhados entre a criacao da chave
/// (ApiKeyEndpointHandlers.Create) e a sua validacao em cada pedido
/// (ApiKeyAuthenticationHandler) - tem de ser exatamente a mesma logica
/// dos dois lados, ou uma chave valida deixa de autenticar.
internal static class ApiKeyHasher
{
    private const string Prefix = "zelo_";

    /// 32 bytes de entropia (>= o que o GitHub usa nos seus PATs) -
    /// Base64Url para ficar seguro em headers HTTP sem escaping.
    public static string GenerateRawKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        return Prefix + token;
    }

    /// So para mostrar na lista - nunca chega para reconstruir a chave.
    public static string DisplayPrefix(string rawKey) => rawKey[..Math.Min(rawKey.Length, Prefix.Length + 8)];

    public static string Hash(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexStringLower(bytes);
    }
}
