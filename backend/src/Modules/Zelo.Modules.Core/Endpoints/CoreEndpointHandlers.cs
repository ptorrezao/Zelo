using Microsoft.EntityFrameworkCore;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Endpoints;

internal static class CoreEndpointHandlers
{
    public static async Task<List<AssetResponse>> GetAssets(Guid householdId, CoreDbContext db, CancellationToken ct) =>
        await db.Assets
            .Where(a => a.HouseholdId == householdId && a.ArchivedAt == null)
            .OrderBy(a => a.Name)
            .Select(a => new AssetResponse(a.Id, a.Module, a.AssetType, a.Name, a.CreatedAt))
            .ToListAsync(ct);

    public static async Task<List<ObligationResponse>> GetObligations(
        Guid householdId, bool? pending, CoreDbContext db, CancellationToken ct)
    {
        var query = db.Obligations.Where(o => o.HouseholdId == householdId);
        if (pending == true)
            query = query.Where(o => o.CompletedOn == null);

        return await query
            .OrderBy(o => o.DueOn)
            .Select(o => new ObligationResponse(o.Id, o.AssetId, o.Module, o.Title, o.DueOn, o.CompletedOn, o.Cost))
            .ToListAsync(ct);
    }
}

internal sealed record AssetResponse(Guid Id, string Module, string AssetType, string Name, DateTimeOffset CreatedAt);

internal sealed record ObligationResponse(
    Guid Id,
    Guid AssetId,
    string Module,
    string Title,
    DateOnly DueOn,
    DateOnly? CompletedOn,
    decimal? Cost);
