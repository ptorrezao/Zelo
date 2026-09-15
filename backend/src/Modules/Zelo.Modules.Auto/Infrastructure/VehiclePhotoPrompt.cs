using Zelo.Modules.Auto.Domain;

namespace Zelo.Modules.Auto.Infrastructure;

/// Prompt em texto livre, independente do fornecedor de geracao de imagem
/// (ver IVehicleImageGenerator) - qualquer provider recebe a mesma string.
internal static class VehiclePhotoPrompt
{
    public static string For(Vehicle vehicle) => vehicle.Category switch
    {
        VehicleCategory.Ligeiros => $"""
            Professional studio product photography of a {vehicle.Registered.Year} {vehicle.Brand} {vehicle.Model} car, {ColorFor(vehicle.Color)} exterior paint.
            Exact side profile view (90-degree lateral shot), vehicle facing right, wheels fully visible.
            Isolated on a plain white background, no surroundings, no reflections of the environment.
            Photorealistic, sharp focus, soft even studio lighting, subtle soft shadow directly beneath the car.
            No text, no watermark, no logos visible, no people.
            Automotive catalog / dealership style, full side silhouette only, not cropped.
            """,
        VehicleCategory.Motociclos => $"""
            Professional studio product photography of a {vehicle.Registered.Year} {vehicle.Brand} {vehicle.Model} motorcycle, {ColorFor(vehicle.Color)} bodywork/tank.
            Exact side profile view (90-degree lateral shot), motorcycle facing right, both wheels fully visible, kickstand hidden or removed digitally.
            Isolated on a plain white background, no surroundings, no reflections of the environment.
            Photorealistic, sharp focus, soft even studio lighting, subtle soft shadow directly beneath the motorcycle.
            No text, no watermark, no logos visible, no people, no riders.
            Automotive catalog / dealership style, full side silhouette only, not cropped.
            """,
        _ => throw new ArgumentOutOfRangeException(nameof(vehicle)),
    };

    // "Cinzento" -> "dark grey (anthracite)", etc. - o modelo responde com
    // mais precisao em ingles; cores fora da lista passam tal e qual (o
    // utilizador pode ja ter escrito em ingles, ou uma cor rara que nao
    // vale a pena mapear).
    private static string ColorFor(string? color) => color is { } c ? (Map.TryGetValue(c, out var en) ? en : c) : "factory-standard";

    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Branco"] = "white", ["Preto"] = "black", ["Cinzento"] = "dark grey (anthracite)",
        ["Vermelho"] = "red", ["Azul"] = "blue", ["Verde"] = "green", ["Prateado"] = "silver",
        ["Dourado"] = "gold", ["Bege"] = "beige", ["Castanho"] = "brown", ["Amarelo"] = "yellow",
        // completar com o que aparecer na pratica - lista pequena, facil de estender
    };
}
