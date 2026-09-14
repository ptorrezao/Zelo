using Microsoft.EntityFrameworkCore;
using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto.Consumers;

/// Reatribui os veiculos de um household eliminado para o predefinido -
/// ver Zelo.Contracts.HouseholdDeleted e HouseholdEndpointHandlers.DeleteHousehold.
internal sealed class HouseholdDeletedHandler(AutoDbContext db) : IEventHandler<HouseholdDeleted>
{
    public async Task HandleAsync(HouseholdDeleted @event, CancellationToken ct)
    {
        var vehicles = await db.Vehicles
            .Where(v => v.HouseholdId == @event.HouseholdId)
            .ToListAsync(ct);

        if (vehicles.Count == 0)
            return;

        foreach (var vehicle in vehicles)
            vehicle.HouseholdId = @event.ReplacementHouseholdId;

        await db.SaveChangesAsync(ct);
    }
}
