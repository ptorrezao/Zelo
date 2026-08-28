using System.Net;

namespace Zelo.ServiceDefaults.Tests;

/// Handler de teste que devolve respostas pre-programadas em vez de bater
/// na rede - permite testar codigo que fala com HttpClient sem precisar
/// de um servidor real a correr.
internal sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public int CallCount { get; private set; }
    public HttpRequestMessage? LastRequest { get; private set; }

    public static FakeHttpMessageHandler ReturningJson(HttpStatusCode status, string json) =>
        new(_ => new HttpResponseMessage(status) { Content = new StringContent(json) });

    public static FakeHttpMessageHandler Throwing(Exception exception) =>
        new(_ => throw exception);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        CallCount++;
        LastRequest = request;
        return Task.FromResult(respond(request));
    }
}
