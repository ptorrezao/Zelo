using System.Net;

namespace Zelo.MigrationRunner.Tests;

/// Handler de teste que despacha por metodo+caminho, para simular um
/// servidor real (Garage/Unleash) sem rede nem containers.
internal sealed class RoutingFakeHttpMessageHandler(
    Func<HttpMethod, string, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<(HttpMethod Method, string Path)> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var path = request.RequestUri!.PathAndQuery;
        Requests.Add((request.Method, path));
        return Task.FromResult(respond(request.Method, path));
    }

    public static HttpResponseMessage Json(HttpStatusCode status, string body) =>
        new(status) { Content = new StringContent(body) };
}
