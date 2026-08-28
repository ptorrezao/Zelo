using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Auto.Application;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Infrastructure;
using Zelo.SharedKernel;

namespace Zelo.Modules.Auto.Endpoints;

/// Handlers extraidos das lambdas inline de AutoEndpoints para serem
/// testaveis diretamente contra AutoDbContext (ex: com EFCore.InMemory),
/// sem precisar de um WebApplicationFactory.
internal static class AutoEndpointHandlers
{
    public static async Task<List<VehicleResponse>> GetVehicles(Guid householdId, AutoDbContext db, CancellationToken ct) =>
        await db.Vehicles
            .Where(v => v.HouseholdId == householdId)
            .OrderBy(v => v.Brand).ThenBy(v => v.Model)
            .Select(v => VehicleResponse.From(v))
            .ToListAsync(ct);

    public static async Task<IResult> CreateVehicle(
        Guid householdId, VehicleUpsertRequest request, AutoDbContext db, IEventPublisher events, CancellationToken ct)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            Category = request.Category,
            Brand = request.Brand,
            Model = request.Model,
            Plate = request.Plate,
            Vin = request.Vin,
            Driver = request.Driver,
            Odometer = request.Odometer,
            Registered = request.Registered,
            NextInspection = request.NextInspection,
            Insurer = request.Insurer,
            InsuranceRenewal = request.InsuranceRenewal,
            IucDueDate = request.IucDueDate,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(ct);

        await events.PublishAsync(VehicleEvents.Created(vehicle), ct);
        if (VehicleEvents.SyncInspectionObligation(vehicle) is { } obligationEvent)
        {
            await db.SaveChangesAsync(ct); // grava o InspectionObligationId atribuido
            await PublishObligationEventAsync(events, obligationEvent, ct);
        }

        return Results.Created($"/api/auto/vehicles/{vehicle.Id}", VehicleResponse.From(vehicle));
    }

    public static async Task<IResult> GetVehicle(Guid id, AutoDbContext db, CancellationToken ct) =>
        await db.Vehicles.FindAsync([id], ct) is { } v ? Results.Ok(VehicleResponse.From(v)) : Results.NotFound();

    public static async Task<IResult> UpdateVehicle(
        Guid id, VehicleUpsertRequest request, AutoDbContext db, IEventPublisher events, CancellationToken ct)
    {
        var vehicle = await db.Vehicles.FindAsync([id], ct);
        if (vehicle is null)
            return Results.NotFound();

        vehicle.Category = request.Category;
        vehicle.Brand = request.Brand;
        vehicle.Model = request.Model;
        vehicle.Plate = request.Plate;
        vehicle.Vin = request.Vin;
        vehicle.Driver = request.Driver;
        vehicle.Odometer = request.Odometer;
        vehicle.Registered = request.Registered;
        vehicle.NextInspection = request.NextInspection;
        vehicle.Insurer = request.Insurer;
        vehicle.InsuranceRenewal = request.InsuranceRenewal;
        vehicle.IucDueDate = request.IucDueDate;

        var obligationEvent = VehicleEvents.SyncInspectionObligation(vehicle);
        await db.SaveChangesAsync(ct);

        if (obligationEvent is not null)
            await PublishObligationEventAsync(events, obligationEvent, ct);

        return Results.Ok(VehicleResponse.From(vehicle));
    }

    public static async Task<IResult> DeleteVehicle(
        Guid id, VehicleStatus status, AutoDbContext db, IEventPublisher events, CancellationToken ct)
    {
        var vehicle = await db.Vehicles.FindAsync([id], ct);
        if (vehicle is null)
            return Results.NotFound();

        vehicle.Status = status;
        await db.SaveChangesAsync(ct);
        await events.PublishAsync(VehicleEvents.Archived(vehicle), ct);

        return Results.NoContent();
    }

    public static async Task<List<MaintenanceResponse>> GetMaintenances(Guid vehicleId, AutoDbContext db, CancellationToken ct) =>
        await db.Maintenances
            .Include(m => m.Items)
            .Where(m => m.VehicleId == vehicleId)
            .OrderByDescending(m => m.Date)
            .Select(m => MaintenanceResponse.From(m))
            .ToListAsync(ct);

    public static async Task<IResult> CreateMaintenance(
        Guid vehicleId, MaintenanceUpsertRequest request, AutoDbContext db, CancellationToken ct)
    {
        if (!await db.Vehicles.AnyAsync(v => v.Id == vehicleId, ct))
            return Results.NotFound();

        var maintenance = new Maintenance
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            Date = request.Date,
            Odometer = request.Odometer,
            Workshop = request.Workshop,
            Description = request.Description,
            Type = request.Type,
            Cost = request.Cost,
            InvoiceNumber = request.InvoiceNumber,
            InvoiceDate = request.InvoiceDate,
        };

        foreach (var item in request.Items ?? [])
        {
            maintenance.Items.Add(new MaintenanceItem
            {
                Id = Guid.NewGuid(),
                MaintenanceId = maintenance.Id,
                Description = item.Description,
                Price = item.Price,
                SerialNumber = item.SerialNumber,
            });
        }

        db.Maintenances.Add(maintenance);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/auto/maintenances/{maintenance.Id}", MaintenanceResponse.From(maintenance));
    }

    public static async Task<IResult> GetMaintenance(Guid id, AutoDbContext db, CancellationToken ct) =>
        await db.Maintenances.Include(m => m.Items).FirstOrDefaultAsync(m => m.Id == id, ct) is { } m
            ? Results.Ok(MaintenanceResponse.From(m))
            : Results.NotFound();

    public static async Task<IResult> UpdateMaintenance(
        Guid id, MaintenanceUpsertRequest request, AutoDbContext db, CancellationToken ct)
    {
        var maintenance = await db.Maintenances.Include(m => m.Items).FirstOrDefaultAsync(m => m.Id == id, ct);
        if (maintenance is null)
            return Results.NotFound();

        maintenance.Date = request.Date;
        maintenance.Odometer = request.Odometer;
        maintenance.Workshop = request.Workshop;
        maintenance.Description = request.Description;
        maintenance.Type = request.Type;
        maintenance.Cost = request.Cost;
        maintenance.InvoiceNumber = request.InvoiceNumber;
        maintenance.InvoiceDate = request.InvoiceDate;

        await db.SaveChangesAsync(ct);
        return Results.Ok(MaintenanceResponse.From(maintenance));
    }

    public static async Task<IResult> DeleteMaintenance(Guid id, AutoDbContext db, CancellationToken ct)
    {
        var maintenance = await db.Maintenances.FindAsync([id], ct);
        if (maintenance is null)
            return Results.NotFound();

        db.Maintenances.Remove(maintenance);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    public static IResult CreateUploadUrl(Guid vehicleId, UploadUrlRequest request, IObjectStorage storage)
    {
        var objectKey = $"vehicles/{vehicleId}/{Guid.NewGuid()}-{request.FileName}";
        var (uploadUrl, expiresAt) = storage.CreateUploadUrl(objectKey, request.ContentType);
        return Results.Ok(new UploadUrlResponse(objectKey, uploadUrl, expiresAt));
    }

    public static async Task<IResult> CreateDocument(
        Guid vehicleId, DocumentCreateRequest request, AutoDbContext db, CancellationToken ct)
    {
        if (!await db.Vehicles.AnyAsync(v => v.Id == vehicleId, ct))
            return Results.NotFound();

        var document = new VehicleDocument
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            Name = request.Name,
            Category = request.Category,
            Type = request.Type,
            Date = request.Date,
            SizeBytes = request.SizeBytes,
            ObjectKey = request.ObjectKey,
            UploadedAt = DateTimeOffset.UtcNow,
        };

        db.Documents.Add(document);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/auto/documents/{document.Id}", DocumentResponse.From(document));
    }

    public static async Task<List<DocumentResponse>> GetDocuments(
        Guid vehicleId, DocumentCategory? category, AutoDbContext db, CancellationToken ct)
    {
        var query = db.Documents.Where(d => d.VehicleId == vehicleId);
        if (category is { } c)
            query = query.Where(d => d.Category == c);

        return await query
            .OrderByDescending(d => d.Date)
            .Select(d => DocumentResponse.From(d))
            .ToListAsync(ct);
    }

    public static async Task<IResult> DeleteDocument(Guid id, AutoDbContext db, CancellationToken ct)
    {
        var document = await db.Documents.FindAsync([id], ct);
        if (document is null)
            return Results.NotFound();

        db.Documents.Remove(document);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    public static async Task<IResult> GetStats(Guid vehicleId, AutoDbContext db, CancellationToken ct)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));

        var recent = await db.Maintenances
            .Where(m => m.VehicleId == vehicleId && m.Date >= since)
            .ToListAsync(ct);

        var vehicle = await db.Vehicles.FindAsync([vehicleId], ct);
        var kmsLastMonth = recent.Count > 0 ? Math.Max(0, (vehicle?.Odometer ?? 0) - recent.Min(m => m.Odometer)) : 0;

        return Results.Ok(new VehicleStatsResponse(
            kmsLastMonth,
            recent.Sum(m => m.Cost),
            recent.Count));
    }

    /// VehicleEvents.SyncInspectionObligation devolve IIntegrationEvent
    /// porque pode ser um de dois tipos concretos - despachar aqui por
    /// switch (em vez de "dynamic") garante que IEventPublisher.PublishAsync
    /// e chamado com o T concreto, que e o que define a routing key AMQP.
    private static Task PublishObligationEventAsync(IEventPublisher events, IIntegrationEvent obligationEvent, CancellationToken ct) =>
        obligationEvent switch
        {
            ObligationScheduled scheduled => events.PublishAsync(scheduled, ct),
            ObligationUpdated updated => events.PublishAsync(updated, ct),
            _ => throw new InvalidOperationException($"Evento de obrigacao inesperado: {obligationEvent.GetType()}"),
        };
}
