using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Endpoints;

public static class CoreEndpoints
{
    // NOTA: householdId vem por query param porque a autenticacao ainda
    // nao esta ligada aos outros modulos (ver IdentityModule) - passar a
    // vir de um claim do utilizador autenticado assim que essa integracao
    // existir.
    public static IEndpointRouteBuilder MapCoreEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/core").RequireAuthorization();

        group.MapGet("/assets", async (Guid householdId, CoreDbContext db, CancellationToken ct) =>
            await db.Assets
                .Where(a => a.HouseholdId == householdId && a.ArchivedAt == null)
                .OrderBy(a => a.Name)
                .Select(a => new AssetResponse(a.Id, a.Module, a.AssetType, a.Name, a.CreatedAt))
                .ToListAsync(ct));

        group.MapGet("/obligations", async (Guid householdId, bool? pending, CoreDbContext db, CancellationToken ct) =>
        {
            var query = db.Obligations.Where(o => o.HouseholdId == householdId);
            if (pending == true)
                query = query.Where(o => o.CompletedOn == null);

            return await query
                .OrderBy(o => o.DueOn)
                .Select(o => new ObligationResponse(o.Id, o.AssetId, o.Module, o.Title, o.DueOn, o.CompletedOn, o.Cost))
                .ToListAsync(ct);
        });

        return app;
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
