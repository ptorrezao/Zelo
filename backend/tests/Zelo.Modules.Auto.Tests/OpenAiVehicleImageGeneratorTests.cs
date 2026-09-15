using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Xunit;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto.Tests;

public class OpenAiVehicleImageGeneratorTests
{
    private static Vehicle NewVehicle() => new()
    {
        Id = Guid.NewGuid(),
        HouseholdId = Guid.NewGuid(),
        Category = VehicleCategory.Ligeiros,
        Brand = "Toyota",
        Model = "Corolla",
        Plate = "AA-00-BB",
        Vin = "VIN123",
        Color = "Branco",
        Registered = new DateOnly(2022, 1, 1),
    };

    private static (OpenAiVehicleImageGenerator Generator, FakeHttpMessageHandler Handler) NewGenerator(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new FakeHttpMessageHandler(respond);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.openai.com") };
        var options = Options.Create(new OpenAiOptions { ApiKey = "sk-test" });
        return (new OpenAiVehicleImageGenerator(http, options), handler);
    }

    [Fact]
    public async Task GenerateVehiclePhotoAsync_EnviaPromptEModeloCorretos()
    {
        var pngBytes = new byte[] { 1, 2, 3, 4 };
        var b64 = Convert.ToBase64String(pngBytes);
        var (generator, handler) = NewGenerator(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($$"""{"data":[{"b64_json":"{{b64}}"}]}"""),
        });

        var result = await generator.GenerateVehiclePhotoAsync(NewVehicle(), CancellationToken.None);

        Assert.Equal(pngBytes, result);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("/v1/images/generations", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal("sk-test", handler.LastRequest.Headers.Authorization.Parameter);

        using var body = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal("gpt-image-1", body.RootElement.GetProperty("model").GetString());
        Assert.Equal("1536x1024", body.RootElement.GetProperty("size").GetString());
        Assert.Contains("Toyota Corolla", body.RootElement.GetProperty("prompt").GetString());
    }

    [Fact]
    public async Task GenerateVehiclePhotoAsync_RespostaComErro_LancaHttpRequestException()
    {
        var (generator, _) = NewGenerator(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests));

        await Assert.ThrowsAsync<HttpRequestException>(
            () => generator.GenerateVehiclePhotoAsync(NewVehicle(), CancellationToken.None));
    }

    [Fact]
    public async Task GenerateVehiclePhotoAsync_RespostaSemImagem_LancaInvalidOperationException()
    {
        var (generator, _) = NewGenerator(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"data":[]}"""),
        });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => generator.GenerateVehiclePhotoAsync(NewVehicle(), CancellationToken.None));
    }
}
