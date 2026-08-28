using System.Collections.Concurrent;
using System.Reflection;

namespace Zelo.Modules.Identity.Infrastructure.Emails;

/// Le os .html embutidos (ver EmbeddedResource no .csproj) uma vez e
/// mantem em memoria - sao ficheiros pequenos, sem custo relevante.
internal static class EmailTemplateLoader
{
    private static readonly ConcurrentDictionary<string, string> Cache = new();
    private static readonly Assembly Assembly = typeof(EmailTemplateLoader).Assembly;

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
