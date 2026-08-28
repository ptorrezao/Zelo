using System.Net;
using Xunit;

namespace Zelo.MigrationRunner.Tests;

public class UnleashBootstrapTests
{
    [Fact]
    public async Task RunAsync_QuandoFlagsNaoExistem_CriaEAtivaTodasAsFlagsEmTodosOsAmbientes()
    {
        var handler = new RoutingFakeHttpMessageHandler((method, path) => (method.Method, path) switch
        {
            ("GET", "/api/admin/projects") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            ("GET", var p) when p.StartsWith("/api/admin/projects/default/features/") =>
                RoutingFakeHttpMessageHandler.Json(HttpStatusCode.NotFound, "{}"),
            ("POST", "/api/admin/projects/default/features") =>
                RoutingFakeHttpMessageHandler.Json(HttpStatusCode.Created, "{}"),
            ("POST", var p) when p.Contains("/environments/") && p.EndsWith("/on") =>
                RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            _ => throw new InvalidOperationException($"pedido inesperado: {method} {path}"),
        });

        await UnleashBootstrap.RunAsync("http://unleash.local", "default:development.secret", handler);

        var createdFlags = handler.Requests.Count(r => r.Method.Method == "POST" && r.Path == "/api/admin/projects/default/features");
        Assert.Equal(UnleashBootstrap.RequiredFlags.Length, createdFlags);

        var enabledCalls = handler.Requests.Count(r => r.Method.Method == "POST" && r.Path.Contains("/environments/") && r.Path.EndsWith("/on"));
        Assert.Equal(UnleashBootstrap.RequiredFlags.Length * 2, enabledCalls); // development + production
    }

    [Fact]
    public async Task RunAsync_QuandoFlagJaExiste_NaoTentaCriarDeNovo()
    {
        var handler = new RoutingFakeHttpMessageHandler((method, path) => (method.Method, path) switch
        {
            ("GET", "/api/admin/projects") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            ("GET", var p) when p.StartsWith("/api/admin/projects/default/features/") =>
                RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            ("POST", var p) when p.Contains("/environments/") && p.EndsWith("/on") =>
                RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            _ => throw new InvalidOperationException($"pedido inesperado: {method} {path}"),
        });

        await UnleashBootstrap.RunAsync("http://unleash.local", "default:development.secret", handler);

        Assert.DoesNotContain(handler.Requests, r => r.Method.Method == "POST" && r.Path == "/api/admin/projects/default/features");
    }

    [Fact]
    public async Task RunAsync_QuandoServidorNuncaFicaPronto_LancaExcecao()
    {
        var handler = new RoutingFakeHttpMessageHandler((_, _) =>
            RoutingFakeHttpMessageHandler.Json(HttpStatusCode.ServiceUnavailable, "{}"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            UnleashBootstrap.RunAsync("http://unleash.local", "default:development.secret", handler));
    }

    [Fact]
    public async Task RunAsync_EnviaAuthorizationSemValidacaoDeEsquema()
    {
        var handler = new RoutingFakeHttpMessageHandler((method, path) => (method.Method, path) switch
        {
            ("GET", "/api/admin/projects") => RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            ("GET", var p) when p.StartsWith("/api/admin/projects/default/features/") =>
                RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            ("POST", var p) when p.Contains("/environments/") && p.EndsWith("/on") =>
                RoutingFakeHttpMessageHandler.Json(HttpStatusCode.OK, "{}"),
            _ => throw new InvalidOperationException($"pedido inesperado: {method} {path}"),
        });

        await UnleashBootstrap.RunAsync("http://unleash.local", "*:*.segredo-sem-esquema", handler);

        Assert.NotEmpty(handler.Requests);
    }
}
