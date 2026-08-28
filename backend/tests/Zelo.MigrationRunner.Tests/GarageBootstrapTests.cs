using System.Net;
using Xunit;

namespace Zelo.MigrationRunner.Tests;

public class GarageBootstrapTests
{
    private const string StatusJson = """{"nodes":[{"id":"node1","isUp":true}]}""";

    [Fact]
    public async Task RunAsync_QuandoNoNaoTemRoleNemBucketNemChave_ConfiguraTudoDeRaiz()
    {
        var handler = new RoutingFakeHttpMessageHandler((method, path) => (method.Method, path) switch
        {
            ("GET", "/v1/status") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, StatusJson),
            ("GET", "/v1/layout") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, """{"version":1,"roles":[]}"""),
            ("POST", "/v1/layout") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            ("POST", "/v1/layout/apply") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            ("GET", var p) when p.StartsWith("/v1/bucket?globalAlias=") =>
                RoutingFakeHttpMessageHandler.Json(HttpStatusCode.NotFound, "{}"),
            ("POST", "/v1/bucket") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, """{"id":"bucket-1"}"""),
            ("GET", var p) when p.StartsWith("/v1/key?id=") =>
                RoutingFakeHttpMessageHandler.Json(HttpStatusCode.NotFound, "{}"),
            ("POST", "/v1/key/import") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            ("POST", "/v1/bucket/allow") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            _ => throw new InvalidOperationException($"pedido inesperado: {method} {path}"),
        });

        await GarageBootstrap.RunAsync(
            "http://garage.local", "admin-token", "zelo-bucket", "access-key", "secret-key", "zelo-api-key", handler);

        Assert.Contains(handler.Requests, r => r.Method.Method == "POST" && r.Path == "/v1/layout");
        Assert.Contains(handler.Requests, r => r.Method.Method == "POST" && r.Path == "/v1/layout/apply");
        Assert.Contains(handler.Requests, r => r.Method.Method == "POST" && r.Path == "/v1/bucket");
        Assert.Contains(handler.Requests, r => r.Method.Method == "POST" && r.Path == "/v1/key/import");
        Assert.Contains(handler.Requests, r => r.Method.Method == "POST" && r.Path == "/v1/bucket/allow");
    }

    [Fact]
    public async Task RunAsync_QuandoNoJaTemRoleBucketEChave_NaoRepeteTrabalho()
    {
        var handler = new RoutingFakeHttpMessageHandler((method, path) => (method.Method, path) switch
        {
            ("GET", "/v1/status") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, StatusJson),
            ("GET", "/v1/layout") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, """{"version":1,"roles":[{"id":"node1"}]}"""),
            ("GET", var p) when p.StartsWith("/v1/bucket?globalAlias=") =>
                RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, """{"id":"bucket-1"}"""),
            ("GET", var p) when p.StartsWith("/v1/key?id=") =>
                RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            ("POST", "/v1/bucket/allow") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            _ => throw new InvalidOperationException($"pedido inesperado: {method} {path}"),
        });

        await GarageBootstrap.RunAsync(
            "http://garage.local", "admin-token", "zelo-bucket", "access-key", "secret-key", "zelo-api-key", handler);

        Assert.DoesNotContain(handler.Requests, r => r.Method.Method == "POST" && r.Path == "/v1/layout");
        Assert.DoesNotContain(handler.Requests, r => r.Method.Method == "POST" && r.Path == "/v1/key/import");
        Assert.Contains(handler.Requests, r => r.Method.Method == "POST" && r.Path == "/v1/bucket/allow");
    }

    [Fact]
    public async Task RunAsync_QuandoGarageNuncaFicaPronto_LancaExcecao()
    {
        var handler = new RoutingFakeHttpMessageHandler((_, _) =>
            RoutingFakeHttpMessageHandler.Json(HttpStatusCode.ServiceUnavailable, "{}"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            GarageBootstrap.RunAsync(
                "http://garage.local", "admin-token", "zelo-bucket", "access-key", "secret-key", "zelo-api-key", handler));
    }
}
