using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Endpoints;

namespace Zelo.Modules.Auto.Infrastructure;

/// Lançada quando o ambiente de origem não pode ser contactado (URL não
/// permitida, falha de autenticação, versão incompatível, resposta
/// inesperada) - o handler mapeia isto para 400 com a mensagem da
/// exceção, sem repetir detalhes do lado remoto ao utilizador.
internal sealed class ImportRemoteException(string message) : Exception(message);

internal interface IImportRemoteClient
{
    /// O utilizador só conhece o site que visita no browser, não o URL da
    /// API por trás dele (são domínios/portas diferentes) - resolve o
    /// segundo a partir do primeiro. Se não conseguir (site mais antigo
    /// sem a rota de descoberta, falha de rede, ou o URL dado já era o da
    /// API), devolve o próprio siteUrl sem alterações.
    Task<Uri> ResolveApiBaseAsync(Uri siteUrl, CancellationToken ct);

    Task<string> LoginAsync(Uri baseUrl, string email, string password, CancellationToken ct);

    Task<IReadOnlyList<ImportHouseholdOption>> GetHouseholdsAsync(Uri baseUrl, string accessToken, CancellationToken ct);

    Task<IReadOnlyList<ImportCandidateVehicle>> GetVehiclesAsync(
        Uri baseUrl, Guid remoteHouseholdId, string accessToken, CancellationToken ct);
}

/// Cliente para o pull de veiculos de outro utilizador - pode estar no
/// mesmo ambiente ou noutro deployment desta mesma app. Corre no backend,
/// nao no browser, para evitar CORS e para as credenciais do utilizador
/// de origem nunca passarem pelo browser de quem importa.
///
/// baseUrl vem do utilizador, por isso todo o pedido passa primeiro por
/// EnsureSafeAsync: so https (exceto localhost em Development) e nunca um
/// IP privado/loopback/link-local, para evitar SSRF contra a propria rede
/// do servidor.
internal sealed class ImportRemoteClient(HttpClient http, IHostEnvironment environment) : IImportRemoteClient
{
    // O ambiente remoto e sempre outra instancia desta mesma app, que
    // serializa enums como string e aceita propriedades em camelCase (ver
    // Program.cs, ConfigureHttpJsonOptions) - JsonSerializerOptions.Default
    // nao tem nenhum dos dois, por isso teria de ser configurado aqui de
    // qualquer forma mesmo com JsonSerializerDefaults.Web.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task<Uri> ResolveApiBaseAsync(Uri siteUrl, CancellationToken ct)
    {
        await EnsureSafeAsync(siteUrl, ct);

        Uri? discovered = null;
        try
        {
            using var response = await http.GetAsync(new Uri(siteUrl, "/api/environment-info"), ct);
            if (response.IsSuccessStatusCode)
            {
                var info = await response.Content.ReadFromJsonAsync<EnvironmentInfoResponse>(JsonOptions, ct);
                if (info is not null && Uri.TryCreate(info.ApiBase, UriKind.Absolute, out var parsed))
                    discovered = parsed;
            }
        }
        catch (HttpRequestException)
        {
            // sem descoberta - segue-se em baixo com o siteUrl original
        }
        catch (JsonException)
        {
            // resposta em /api/environment-info nao e o que esperavamos -
            // idem, segue-se com o siteUrl original
        }

        if (discovered is null)
            return siteUrl;

        await EnsureSafeAsync(discovered, ct);
        return discovered;
    }

    public async Task<string> LoginAsync(Uri baseUrl, string email, string password, CancellationToken ct)
    {
        await EnsureSafeAsync(baseUrl, ct);

        using var response = await http.PostAsJsonAsync(
            new Uri(baseUrl, "/api/auth/login?useCookies=false"), new { email, password }, JsonOptions, ct);

        // Se mesmo depois da descoberta em ResolveApiBaseAsync isto ainda
        // devolver um redirect, o URL dado nao e mesmo o de um ambiente
        // Zelo (ou e uma versao sem a rota de descoberta E sem
        // /api/auth/login a funcionar como esperado).
        if ((int)response.StatusCode is >= 300 and < 400)
        {
            throw new ImportRemoteException(
                "O URL indicado não respondeu como um ambiente Zelo (devolveu um redirecionamento) — confirme o URL do site de origem.");
        }

        if (!response.IsSuccessStatusCode)
            throw new ImportRemoteException("Não foi possível autenticar no ambiente de origem.");

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, ct);
        if (payload is null || string.IsNullOrEmpty(payload.AccessToken))
            throw new ImportRemoteException("Resposta inválida do ambiente de origem.");

        return payload.AccessToken;
    }

    public async Task<IReadOnlyList<ImportHouseholdOption>> GetHouseholdsAsync(Uri baseUrl, string accessToken, CancellationToken ct)
    {
        await EnsureSafeAsync(baseUrl, ct);

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUrl, "/api/v1/households/me"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new ImportRemoteException(
                "Este ambiente de origem está numa versão mais antiga e ainda não suporta importação — atualiza-o primeiro.");
        }

        if (!response.IsSuccessStatusCode)
            throw new ImportRemoteException("Não foi possível obter os households do ambiente de origem.");

        var households = await response.Content.ReadFromJsonAsync<List<ImportHouseholdOption>>(JsonOptions, ct) ?? [];
        return households;
    }

    public async Task<IReadOnlyList<ImportCandidateVehicle>> GetVehiclesAsync(
        Uri baseUrl, Guid remoteHouseholdId, string accessToken, CancellationToken ct)
    {
        await EnsureSafeAsync(baseUrl, ct);

        using var request = new HttpRequestMessage(
            HttpMethod.Get, new Uri(baseUrl, $"/api/auto/vehicles?householdId={remoteHouseholdId}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            throw new ImportRemoteException("Não foi possível obter os veículos do ambiente de origem.");

        var vehicles = await response.Content.ReadFromJsonAsync<List<RemoteVehicleResponse>>(JsonOptions, ct) ?? [];
        return [.. vehicles.Select(v => new ImportCandidateVehicle(
            v.Category, v.Brand, v.Model, v.Plate, v.Vin, v.Color, v.Driver, v.Odometer,
            v.Registered, v.NextInspection, v.Insurer, v.InsurancePolicyNumber,
            v.InsurancePeriodStart, v.InsurancePeriodEnd, v.InsurancePremium, v.IucDueDate))];
    }

    private async Task EnsureSafeAsync(Uri baseUrl, CancellationToken ct)
    {
        var isLocalhost = string.Equals(baseUrl.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            || IPAddress.TryParse(baseUrl.Host, out var literal) && IPAddress.IsLoopback(literal);

        var allowPlainHttp = environment.IsDevelopment() && isLocalhost;
        if (!string.Equals(baseUrl.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) && !allowPlainHttp)
            throw new ImportRemoteException("O ambiente de origem tem de usar https.");

        if (allowPlainHttp)
            return; // localhost em dev - nao vale a pena resolver DNS nem bloquear IPs privados aqui

        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(baseUrl.Host, ct);
        }
        catch (SocketException)
        {
            throw new ImportRemoteException("Não foi possível resolver o endereço do ambiente de origem.");
        }

        if (addresses.Length == 0 || addresses.Any(IsDisallowedAddress))
            throw new ImportRemoteException("Endereço do ambiente de origem não permitido.");
    }

    private static bool IsDisallowedAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
            return true;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            return bytes[0] == 10 // 10.0.0.0/8
                || (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) // 172.16.0.0/12
                || (bytes[0] == 192 && bytes[1] == 168) // 192.168.0.0/16
                || (bytes[0] == 169 && bytes[1] == 254); // 169.254.0.0/16 (link-local)
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (address.IsIPv6LinkLocal)
                return true;

            var bytes = address.GetAddressBytes();
            return (bytes[0] & 0xfe) == 0xfc; // fc00::/7 (unique local)
        }

        return false;
    }

    private sealed record EnvironmentInfoResponse(string ApiBase);

    private sealed record LoginResponse(string AccessToken);

    private sealed record RemoteVehicleResponse(
        VehicleCategory Category,
        string Brand,
        string Model,
        string Plate,
        string Vin,
        string? Color,
        string? Driver,
        int Odometer,
        DateOnly Registered,
        DateOnly? NextInspection,
        string? Insurer,
        string? InsurancePolicyNumber,
        DateOnly? InsurancePeriodStart,
        DateOnly? InsurancePeriodEnd,
        decimal? InsurancePremium,
        DateOnly? IucDueDate);
}
