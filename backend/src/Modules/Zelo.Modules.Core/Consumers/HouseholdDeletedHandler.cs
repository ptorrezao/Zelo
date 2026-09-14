using Microsoft.EntityFrameworkCore;
using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Consumers;

/// Reatribui os ativos e obrigacoes de um household eliminado para o
/// predefinido - ver Zelo.Contracts.HouseholdDeleted e
/// HouseholdEndpointHandlers.DeleteHousehold, no modulo Identity.
internal sealed class HouseholdDeletedHandler(CoreDbContext db) : IEventHandler<HouseholdDeleted>
{
    public async Task HandleAsync(HouseholdDeleted @event, CancellationToken ct)
    {
        var assets = await db.Assets
            .Where(a => a.HouseholdId == @event.HouseholdId)
            .ToListAsync(ct);
        foreach (var asset in assets)
            asset.HouseholdId = @event.ReplacementHouseholdId;

        var obligations = await db.Obligations
            .Where(o => o.HouseholdId == @event.HouseholdId)
            .ToListAsync(ct);
        foreach (var obligation in obligations)
            obligation.HouseholdId = @event.ReplacementHouseholdId;

        if (assets.Count > 0 || obligations.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
