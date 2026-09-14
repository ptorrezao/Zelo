using System.Reflection;
using System.Text.Json;

namespace Zelo.Modules.Auto.Infrastructure.VehicleCatalog;

/// Le o vehicle-catalog.json embutido (ver EmbeddedResource no .csproj)
/// uma vez e mantem em memoria - ficheiro pequeno, sem custo relevante em
/// re-lê-lo a cada pedido, mas nao ha razao para o fazer.
internal static class VehicleCatalogLoader
{
    private static readonly Lazy<IReadOnlyDictionary<string, string[]>> Catalog = new(Load);

    public static IReadOnlyDictionary<string, string[]> Get() => Catalog.Value;

    private static IReadOnlyDictionary<string, string[]> Load()
    {
        var assembly = typeof(VehicleCatalogLoader).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(n => n.EndsWith(".vehicle-catalog.json", StringComparison.Ordinal))
            ?? throw new InvalidOperationException("vehicle-catalog.json nao encontrado como EmbeddedResource.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        return JsonSerializer.Deserialize<Dictionary<string, string[]>>(stream)
            ?? throw new InvalidOperationException("vehicle-catalog.json vazio ou invalido.");
    }
}
