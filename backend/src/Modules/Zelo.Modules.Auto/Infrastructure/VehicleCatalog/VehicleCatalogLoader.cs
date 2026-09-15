using System.Reflection;
using System.Text.Json;

namespace Zelo.Modules.Auto.Infrastructure.VehicleCatalog;

/// Le o vehicle-catalog.json embutido (ver EmbeddedResource no .csproj)
/// uma vez e mantem em memoria - ficheiro pequeno, sem custo relevante em
/// re-lê-lo a cada pedido, mas nao ha razao para o fazer.
///
/// Estrutura: categoria ("Ligeiros"/"Motociclos", mesmas chaves de
/// VehicleCategory) -> marca -> modelos. Uma marca como a BMW ou a Honda
/// pode aparecer nas duas categorias com listas de modelos diferentes -
/// sem isto, escolher "Motociclo" continuava a sugerir carros da mesma
/// marca (e vice-versa).
internal static class VehicleCatalogLoader
{
    private static readonly Lazy<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string[]>>> Catalog = new(Load);

    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string[]>> Get() => Catalog.Value;

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string[]>> Load()
    {
        var assembly = typeof(VehicleCatalogLoader).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(n => n.EndsWith(".vehicle-catalog.json", StringComparison.Ordinal))
            ?? throw new InvalidOperationException("vehicle-catalog.json nao encontrado como EmbeddedResource.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        var parsed = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string[]>>>(stream)
            ?? throw new InvalidOperationException("vehicle-catalog.json vazio ou invalido.");

        return parsed.ToDictionary(
            categoryEntry => categoryEntry.Key,
            IReadOnlyDictionary<string, string[]> (categoryEntry) => categoryEntry.Value);
    }
}
