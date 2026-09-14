using Zelo.Modules.Auto.Endpoints;
using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto.Tests;

internal sealed class FakeImportRemoteClient : IImportRemoteClient
{
    public string AccessToken { get; set; } = "fake-token";
    public IReadOnlyList<ImportHouseholdOption> Households { get; set; } = [];
    public IReadOnlyList<ImportCandidateVehicle> Vehicles { get; set; } = [];
    public ImportRemoteException? FailWith { get; set; }

    public Task<Uri> ResolveApiBaseAsync(Uri siteUrl, CancellationToken ct) =>
        FailWith is null ? Task.FromResult(siteUrl) : Task.FromException<Uri>(FailWith);

    public Task<string> LoginAsync(Uri baseUrl, string email, string password, CancellationToken ct) =>
        FailWith is null ? Task.FromResult(AccessToken) : Task.FromException<string>(FailWith);

    public Task<IReadOnlyList<ImportHouseholdOption>> GetHouseholdsAsync(Uri baseUrl, string accessToken, CancellationToken ct) =>
        FailWith is null ? Task.FromResult(Households) : Task.FromException<IReadOnlyList<ImportHouseholdOption>>(FailWith);

    public Task<IReadOnlyList<ImportCandidateVehicle>> GetVehiclesAsync(
        Uri baseUrl, Guid remoteHouseholdId, string accessToken, CancellationToken ct) =>
        FailWith is null ? Task.FromResult(Vehicles) : Task.FromException<IReadOnlyList<ImportCandidateVehicle>>(FailWith);
}
