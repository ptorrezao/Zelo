namespace Zelo.Modules.Auto.Tests;

/// Handler de teste que devolve respostas pre-programadas em vez de bater
/// na rede - permite testar codigo que fala com HttpClient sem precisar de
/// um servidor real a correr.
internal sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        LastRequest = request;
        LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
        return respond(request);
    }
}
