using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Zelo.Modules.Core.Domain;
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

    public static async Task<List<NotificationResponse>> GetNotifications(
        Guid householdId, bool? unacknowledgedOnly, CoreDbContext db, CancellationToken ct)
    {
        var query = db.NotificationLogs.Where(l => l.HouseholdId == householdId);
        if (unacknowledgedOnly == true)
            query = query.Where(l => l.AcknowledgedAt == null);

        return await query
            .OrderByDescending(l => l.TriggeredAt)
            .Join(db.Obligations, l => l.ObligationId, o => o.Id,
                (l, o) => new NotificationResponse(l.Id, l.ObligationId, o.Title, o.DueOn, l.DaysUntilDue, l.TriggeredAt, l.AcknowledgedAt))
            .ToListAsync(ct);
    }

    public static async Task<IResult> AcknowledgeNotification(
        Guid householdId, Guid id, CoreDbContext db, CancellationToken ct)
    {
        var log = await db.NotificationLogs.SingleOrDefaultAsync(l => l.Id == id && l.HouseholdId == householdId, ct);
        if (log is null)
            return Results.NotFound();

        log.AcknowledgedAt ??= DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    public static async Task<NotificationPreferenceResponse> GetPreferences(Guid householdId, CoreDbContext db, CancellationToken ct)
    {
        var pref = await db.NotificationPreferences.FindAsync([householdId], ct);
        return pref is null
            ? new NotificationPreferenceResponse(15, true)
            : new NotificationPreferenceResponse(pref.DaysWarning, pref.EmailEnabled);
    }

    public static async Task<NotificationPreferenceResponse> UpdatePreferences(
        Guid householdId, NotificationPreferenceRequest request, CoreDbContext db, CancellationToken ct)
    {
        var daysWarning = Math.Clamp(request.DaysWarning, 1, 90);

        var pref = await db.NotificationPreferences.FindAsync([householdId], ct);
        if (pref is null)
        {
            pref = new NotificationPreference { HouseholdId = householdId };
            db.NotificationPreferences.Add(pref);
        }

        pref.DaysWarning = daysWarning;
        pref.EmailEnabled = request.EmailEnabled;
        await db.SaveChangesAsync(ct);

        return new NotificationPreferenceResponse(pref.DaysWarning, pref.EmailEnabled);
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

internal sealed record NotificationResponse(
    Guid Id,
    Guid ObligationId,
    string Title,
    DateOnly DueOn,
    int DaysUntilDue,
    DateTimeOffset TriggeredAt,
    DateTimeOffset? AcknowledgedAt);

internal sealed record NotificationPreferenceResponse(int DaysWarning, bool EmailEnabled);

internal sealed record NotificationPreferenceRequest(int DaysWarning, bool EmailEnabled);
