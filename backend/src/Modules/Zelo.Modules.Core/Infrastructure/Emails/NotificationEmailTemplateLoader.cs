using System.Collections.Concurrent;
using System.Reflection;

namespace Zelo.Modules.Core.Infrastructure.Emails;

/// Le os .html embutidos (ver EmbeddedResource no .csproj) uma vez e
/// mantem em memoria - sao ficheiros pequenos, sem custo relevante.
/// Copia deliberada do equivalente em Zelo.Modules.Identity: as classes
/// de la sao internal ao modulo, e Core nao pode depender de Identity
/// (module boundary).
internal static class NotificationEmailTemplateLoader
{
    private static readonly ConcurrentDictionary<string, string> Cache = new();
    private static readonly Assembly Assembly = typeof(NotificationEmailTemplateLoader).Assembly;

    public static string Load(string fileName) =>
        Cache.GetOrAdd(fileName, static name =>
        {
            var resourceName = Assembly.GetManifestResourceNames()
                .SingleOrDefault(n => n.EndsWith("." + name, StringComparison.Ordinal))
                ?? throw new InvalidOperationException(
                    $"Template de email '{name}' nao encontrado como EmbeddedResource.");

            using var stream = Assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        });
}
