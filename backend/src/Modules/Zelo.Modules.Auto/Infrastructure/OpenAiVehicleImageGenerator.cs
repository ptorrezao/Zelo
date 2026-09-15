using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Zelo.Modules.Auto.Domain;

namespace Zelo.Modules.Auto.Infrastructure;

/// gpt-image-1 via a API Images normal (nao a Batch API - essa e mais
/// barata mas assincrona, horas de atraso, nao compensa ao volume deste
/// projeto). Devolve o PNG tal como a OpenAI o gera (1536x1024) - sem
/// recorte/resize: VehiclePhoto.vue (frontend) usa `height: auto`, por
/// isso qualquer proporcao encaixa sem distorcer.
internal sealed class OpenAiVehicleImageGenerator(HttpClient http, IOptions<OpenAiOptions> options) : IVehicleImageGenerator
{
    public async Task<byte[]> GenerateVehiclePhotoAsync(Vehicle vehicle, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/images/generations")
        {
            Content = JsonContent.Create(new OpenAiImageRequest(
                Model: "gpt-image-1",
                Prompt: VehiclePhotoPrompt.For(vehicle),
                Size: "1536x1024",
                Background: "transparent",
                N: 1)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);

        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<OpenAiImageResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Resposta vazia da OpenAI ao gerar a foto do veiculo.");
        var b64 = body.Data.FirstOrDefault()?.B64Json
            ?? throw new InvalidOperationException("Resposta da OpenAI sem imagem gerada.");

        return Convert.FromBase64String(b64);
    }

    private sealed record OpenAiImageRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("size")] string Size,
        [property: JsonPropertyName("background")] string Background,
        [property: JsonPropertyName("n")] int N);

    private sealed record OpenAiImageResponse([property: JsonPropertyName("data")] List<OpenAiImageData> Data);

    private sealed record OpenAiImageData([property: JsonPropertyName("b64_json")] string? B64Json);
}
