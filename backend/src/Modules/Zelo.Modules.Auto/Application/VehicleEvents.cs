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

    public static AssetCreated Created(Vehicle vehicle) => new(
        Guid.NewGuid(), DateTimeOffset.UtcNow, vehicle.Id, vehicle.HouseholdId,
        ModuleKey, "vehicle", $"{vehicle.Brand} {vehicle.Model} ({vehicle.Plate})");

    public static AssetArchived Archived(Vehicle vehicle) => new(
        Guid.NewGuid(), DateTimeOffset.UtcNow, vehicle.Id, vehicle.HouseholdId);

    /// Devolve o evento a publicar para refletir o NextInspection atual do
    /// veiculo, ou null se nao houver nada a publicar (sem data e sem
    /// obrigacao previa). Atribui vehicle.InspectionObligationId na
    /// primeira vez - o chamador tem de gravar o veiculo depois.
    public static IIntegrationEvent? SyncInspectionObligation(Vehicle vehicle)
    {
        if (vehicle.NextInspection is not { } dueOn)
            return null;

        var title = $"{InspectionTitlePrefix} - {vehicle.Brand} {vehicle.Model} ({vehicle.Plate})";

        if (vehicle.InspectionObligationId is not { } obligationId)
        {
            obligationId = Guid.NewGuid();
            vehicle.InspectionObligationId = obligationId;
            return new ObligationScheduled(
                Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, vehicle.Id, vehicle.HouseholdId,
                ModuleKey, title, dueOn);
        }

        return new ObligationUpdated(
            Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, vehicle.HouseholdId, title, dueOn);
    }
}
