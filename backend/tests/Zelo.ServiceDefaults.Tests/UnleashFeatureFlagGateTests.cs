using System.Net;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Zelo.ServiceDefaults.Tests;

public class UnleashFeatureFlagGateTests
{
    private static UnleashFeatureFlagGate CreateGate(FakeHttpMessageHandler handler, FeatureFlagsOptions? options = null)
    {
        options ??= new FeatureFlagsOptions
        {
            Url = "http://unleash.local",
            ApiToken = "default:development.secret",
            Environment = "development",
        };

        var httpClient = new HttpClient(handler);
        if (options.Url is not null)
            httpClient.BaseAddress = new Uri(options.Url);
        return new UnleashFeatureFlagGate(httpClient, Options.Create(options), NullLogger<UnleashFeatureFlagGate>.Instance);
    }

    [Fact]
    public async Task IsEnabledAsync_SemUrlConfigurado_DevolveTrueSemChamarHttp()
    {
        var handler = FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "{}");
        var gate = CreateGate(handler, new FeatureFlagsOptions { Url = null, ApiToken = null });

        var result = await gate.IsEnabledAsync("auto-app-enabled");

        Assert.True(result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task IsEnabledAsync_FlagAtivaNoAmbiente_DevolveTrue()
    {
        var json = """{"environments":[{"name":"development","enabled":true}]}""";
        var handler = FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, json);
        var gate = CreateGate(handler);

        var result = await gate.IsEnabledAsync("auto-app-enabled");

        Assert.True(result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task IsEnabledAsync_FlagDesativadaNoAmbiente_DevolveFalse()
    {
        var json = """{"environments":[{"name":"development","enabled":false}]}""";
        var handler = FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, json);
        var gate = CreateGate(handler);

        var result = await gate.IsEnabledAsync("inventory-app-enabled");

        Assert.False(result);
    }

    [Fact]
    public async Task IsEnabledAsync_AmbienteNaoEncontrado_DevolveTrue()
    {
        var json = """{"environments":[{"name":"production","enabled":false}]}""";
        var handler = FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, json);
        var gate = CreateGate(handler);

        var result = await gate.IsEnabledAsync("auto-app-enabled");

        Assert.True(result);
    }

    [Fact]
    public async Task IsEnabledAsync_RespostaComErro_DevolveTrue()
    {
        var handler = FakeHttpMessageHandler.ReturningJson(HttpStatusCode.NotFound, "{}");
        var gate = CreateGate(handler);

        var result = await gate.IsEnabledAsync("auto-app-enabled");

        Assert.True(result);
    }

    [Fact]
    public async Task IsEnabledAsync_ExcecaoDeRede_DevolveTrue()
    {
        var handler = FakeHttpMessageHandler.Throwing(new HttpRequestException("falha de rede"));
        var gate = CreateGate(handler);

        var result = await gate.IsEnabledAsync("auto-app-enabled");

        Assert.True(result);
    }

    [Fact]
    public async Task IsEnabledAsync_Timeout_DevolveTrue()
    {
        var handler = FakeHttpMessageHandler.Throwing(new TaskCanceledException("timeout"));
        var gate = CreateGate(handler);

        var result = await gate.IsEnabledAsync("auto-app-enabled");

        Assert.True(result);
    }

    [Fact]
    public async Task IsEnabledAsync_ChamadasRepetidas_UsaCacheEmVezDeNovoPedidoHttp()
    {
        var json = """{"environments":[{"name":"development","enabled":true}]}""";
        var handler = FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, json);
        var gate = CreateGate(handler);

        await gate.IsEnabledAsync("auto-app-enabled");
        await gate.IsEnabledAsync("auto-app-enabled");
        await gate.IsEnabledAsync("auto-app-enabled");

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task IsEnabledAsync_FlagsDiferentes_ChamamHttpSeparadamente()
    {
        var json = """{"environments":[{"name":"development","enabled":true}]}""";
        var handler = FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, json);
        var gate = CreateGate(handler);

        await gate.IsEnabledAsync("auto-app-enabled");
        await gate.IsEnabledAsync("inventory-app-enabled");

        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task IsEnabledAsync_EnviaAuthorizationComOApiToken()
    {
        var json = """{"environments":[{"name":"development","enabled":true}]}""";
        var handler = FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, json);
        var gate = CreateGate(handler);

        await gate.IsEnabledAsync("auto-app-enabled");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("default:development.secret", handler.LastRequest!.Headers.GetValues("Authorization").Single());
    }
}
