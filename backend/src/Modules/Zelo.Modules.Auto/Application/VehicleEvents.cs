using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Auto.Domain;
using Zelo.SharedKernel;

namespace Zelo.Modules.Auto.Application;

/// Traduz mudancas ao veiculo para os eventos definidos em Zelo.Contracts.
/// Nenhum outro sitio do modulo publica eventos diretamente - mantem a
/// logica de "quando e que uma obrigacao nasce/reagenda" num so lugar.
internal static class VehicleEvents
{
    private const string ModuleKey = "auto";
    private const string InspectionTitlePrefix = "Inspecao periodica";
    private const string InsuranceTitlePrefix = "Renovação do seguro";

    public static AssetCreated Created(Vehicle vehicle) => new(
        Guid.NewGuid(), DateTimeOffset.UtcNow, vehicle.Id, vehicle.HouseholdId,
        ModuleKey, "vehicle", $"{vehicle.Brand} {vehicle.Model} ({vehicle.Plate})");

    public static AssetArchived Archived(Vehicle vehicle) => new(
        Guid.NewGuid(), DateTimeOffset.UtcNow, vehicle.Id, vehicle.HouseholdId);

    /// Devolve o evento a publicar para refletir o NextInspection atual do
    /// veiculo, ou null se nao houver nada a publicar (sem data e sem
    /// obrigacao previa). Atribui vehicle.InspectionObligationId na
    /// primeira vez - o chamador tem de gravar o veiculo depois.
    public static IIntegrationEvent? SyncInspectionObligation(Vehicle vehicle) =>
        SyncObligation(vehicle, vehicle.NextInspection, InspectionTitlePrefix,
            () => vehicle.InspectionObligationId, id => vehicle.InspectionObligationId = id);

    /// Mesmo padrao que SyncInspectionObligation, a partir de
    /// InsurancePeriodEnd.
    public static IIntegrationEvent? SyncInsuranceObligation(Vehicle vehicle) =>
        SyncObligation(vehicle, vehicle.InsurancePeriodEnd, InsuranceTitlePrefix,
            () => vehicle.InsuranceObligationId, id => vehicle.InsuranceObligationId = id);

    private static IIntegrationEvent? SyncObligation(
        Vehicle vehicle, DateOnly? dueOn, string titlePrefix,
        Func<Guid?> getObligationId, Action<Guid> setObligationId)
    {
        if (dueOn is not { } due)
            return null;

        var title = $"{titlePrefix} - {vehicle.Brand} {vehicle.Model} ({vehicle.Plate})";

        if (getObligationId() is not { } obligationId)
        {
            obligationId = Guid.NewGuid();
            setObligationId(obligationId);
            return new ObligationScheduled(
                Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, vehicle.Id, vehicle.HouseholdId,
                ModuleKey, title, due);
        }

        return new ObligationUpdated(
            Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, vehicle.HouseholdId, title, due);
    }
}
