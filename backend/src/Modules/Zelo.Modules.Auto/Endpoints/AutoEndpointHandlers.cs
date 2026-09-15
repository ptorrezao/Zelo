using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Auto.Application;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Infrastructure;
using Zelo.Modules.Auto.Infrastructure.VehicleCatalog;
using Zelo.SharedKernel;

namespace Zelo.Modules.Auto.Endpoints;

/// Handlers extraidos das lambdas inline de AutoEndpoints para serem
/// testaveis diretamente contra AutoDbContext (ex: com EFCore.InMemory),
/// sem precisar de um WebApplicationFactory.
internal static class AutoEndpointHandlers
{
    public static IResult GetVehicleCatalog() => Results.Ok(VehicleCatalogLoader.Get());

    public static async Task<List<VehicleResponse>> GetVehicles(Guid householdId, AutoDbContext db, IObjectStorage storage, CancellationToken ct)
    {
        var vehicles = await db.Vehicles
            .Where(v => v.HouseholdId == householdId)
            .OrderBy(v => v.Brand).ThenBy(v => v.Model)
            .ToListAsync(ct);
        return [.. vehicles.Select(v => VehicleResponse.From(v, storage))];
    }

    public static async Task<IResult> CreateVehicle(
        Guid householdId, VehicleUpsertRequest request, AutoDbContext db, IEventPublisher events, IObjectStorage storage, CancellationToken ct)
    {
        try
        {
            var vehicle = await CreateVehicleEntityAsync(householdId, request, db, events, ct);
            return Results.Created($"/api/auto/vehicles/{vehicle.Id}", VehicleResponse.From(vehicle, storage));
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    /// Construcao/persistencia partilhada entre CreateVehicle, ConfirmImport
    /// e AutoMcpTools.CreateVehicle - todas precisam exatamente da mesma
    /// logica (entidade + sync de inspecao + eventos + validacao), so a
    /// primeira devolve um IResult. Lanca ArgumentException (ver
    /// VehicleValidation) - quem chama decide como traduzir isso.
    internal static async Task<Vehicle> CreateVehicleEntityAsync(
        Guid householdId, VehicleUpsertRequest request, AutoDbContext db, IEventPublisher events, CancellationToken ct)
    {
        VehicleValidation.Validate(request);

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            Category = request.Category,
            Brand = request.Brand,
            Model = request.Model,
            Plate = request.Plate,
            Vin = request.Vin,
            Color = request.Color,
            Driver = request.Driver,
            Odometer = request.Odometer,
            Registered = request.Registered,
            NextInspection = request.NextInspection,
            Insurer = request.Insurer,
            InsurancePolicyNumber = request.InsurancePolicyNumber,
            InsurancePeriodStart = request.InsurancePeriodStart,
            InsurancePeriodEnd = request.InsurancePeriodEnd,
            InsurancePremium = request.InsurancePremium,
            IucDueDate = request.IucDueDate,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.Vehicles.Add(vehicle);
        await SaveOrThrowOnDuplicatePlateAsync(db, ct);

        await events.PublishAsync(VehicleEvents.Created(vehicle), ct);
        var inspectionEvent = VehicleEvents.SyncInspectionObligation(vehicle);
        var insuranceEvent = VehicleEvents.SyncInsuranceObligation(vehicle);
        if (inspectionEvent is not null || insuranceEvent is not null)
            await db.SaveChangesAsync(ct); // grava o(s) ObligationId atribuido(s)
        if (inspectionEvent is not null)
            await PublishObligationEventAsync(events, inspectionEvent, ct);
        if (insuranceEvent is not null)
            await PublishObligationEventAsync(events, insuranceEvent, ct);

        return vehicle;
    }

    public static async Task<IResult> GetVehicle(Guid id, AutoDbContext db, IObjectStorage storage, CancellationToken ct) =>
        await db.Vehicles.FindAsync([id], ct) is { } v ? Results.Ok(VehicleResponse.From(v, storage)) : Results.NotFound();

    public static async Task<IResult> UpdateVehicle(
        Guid id, VehicleUpsertRequest request, AutoDbContext db, IEventPublisher events, IObjectStorage storage, CancellationToken ct)
    {
        try
        {
            var vehicle = await UpdateVehicleEntityAsync(id, request, db, events, ct);
            return vehicle is null ? Results.NotFound() : Results.Ok(VehicleResponse.From(vehicle, storage));
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    /// Partilhada com AutoMcpTools.UpdateVehicle - devolve null (em vez de
    /// IResult) para quem chama nao depender de tipos de Minimal API. Lanca
    /// ArgumentException (ver VehicleValidation) - quem chama decide como
    /// traduzir isso.
    internal static async Task<Vehicle?> UpdateVehicleEntityAsync(
        Guid id, VehicleUpsertRequest request, AutoDbContext db, IEventPublisher events, CancellationToken ct)
    {
        var vehicle = await db.Vehicles.FindAsync([id], ct);
        if (vehicle is null)
            return null;

        VehicleValidation.Validate(request);

        // Mudar a cor torna a foto gerada (presa a cor da criacao) errada -
        // apaga a referencia e volta a publicar AssetCreated para o
        // VehiclePhotoHandler gerar de novo. Os dois consumidores deste
        // evento sao idempotentes por AssetId (ver AssetCreatedHandler no
        // Core, e o guard "ja tem foto" no VehiclePhotoHandler), por isso
        // reusar o mesmo evento em vez de um VehicleUpdated proprio e seguro.
        // ponytail: nao apaga o objecto antigo no Garage (fica orfao) -
        // limpar isso exigiria acompanhar a expiracao do bucket, sem valor
        // imediato aqui.
        var colorChanged = !string.Equals(vehicle.Color, request.Color, StringComparison.Ordinal);

        vehicle.Category = request.Category;
        vehicle.Brand = request.Brand;
        vehicle.Model = request.Model;
        vehicle.Plate = request.Plate;
        vehicle.Vin = request.Vin;
        vehicle.Color = request.Color;
        vehicle.Driver = request.Driver;
        vehicle.Odometer = request.Odometer;
        vehicle.Registered = request.Registered;
        vehicle.NextInspection = request.NextInspection;
        vehicle.Insurer = request.Insurer;
        vehicle.InsurancePolicyNumber = request.InsurancePolicyNumber;
        vehicle.InsurancePeriodStart = request.InsurancePeriodStart;
        vehicle.InsurancePeriodEnd = request.InsurancePeriodEnd;
        vehicle.InsurancePremium = request.InsurancePremium;
        vehicle.IucDueDate = request.IucDueDate;
        if (colorChanged)
            vehicle.PhotoObjectKey = null;

        var inspectionEvent = VehicleEvents.SyncInspectionObligation(vehicle);
        var insuranceEvent = VehicleEvents.SyncInsuranceObligation(vehicle);
        await SaveOrThrowOnDuplicatePlateAsync(db, ct);

        if (inspectionEvent is not null)
            await PublishObligationEventAsync(events, inspectionEvent, ct);
        if (insuranceEvent is not null)
            await PublishObligationEventAsync(events, insuranceEvent, ct);
        if (colorChanged)
            await events.PublishAsync(VehicleEvents.Created(vehicle), ct);

        return vehicle;
    }

    /// A unicidade de (HouseholdId, Plate) e garantida por indice na BD
    /// (ver VehicleConfiguration) - sem isto, duplicar uma matricula dava
    /// um DbUpdateException/500 em vez de uma mensagem clara.
    private static async Task SaveOrThrowOnDuplicatePlateAsync(AutoDbContext db, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ArgumentException("Matrícula já existe neste household.");
        }
    }

    public static async Task<IResult> DeleteVehicle(
        Guid id, VehicleStatus status, AutoDbContext db, IEventPublisher events, CancellationToken ct)
    {
        var vehicle = await ArchiveVehicleEntityAsync(id, status, db, events, ct);
        return vehicle is null ? Results.NotFound() : Results.NoContent();
    }

    /// Partilhada com AutoMcpTools.DeleteVehicle - "delete" e sempre uma
    /// mudanca de estado (Vendido/Abatido), nunca um DELETE fisico.
    internal static async Task<Vehicle?> ArchiveVehicleEntityAsync(
        Guid id, VehicleStatus status, AutoDbContext db, IEventPublisher events, CancellationToken ct)
    {
        var vehicle = await db.Vehicles.FindAsync([id], ct);
        if (vehicle is null)
            return null;

        vehicle.Status = status;
        await db.SaveChangesAsync(ct);
        await events.PublishAsync(VehicleEvents.Archived(vehicle), ct);

        return vehicle;
    }

    public static async Task<IResult> PreviewImport(
        Guid householdId, ImportConnectRequest request, ClaimsPrincipal caller,
        HttpContext httpContext, AutoDbContext db, IImportRemoteClient remoteClient, CancellationToken ct)
    {
        if (!Uri.TryCreate(request.BaseUrl, UriKind.Absolute, out var baseUrl))
            return Results.BadRequest(new { error = "URL do ambiente de origem inválido." });

        try
        {
            // O utilizador cola o URL do site que usa (ex. https://auto.zelo.pt),
            // nao o da API por trás dele - resolve-se aqui automaticamente
            // (ver environment-info.get.ts no frontend). Se a descoberta
            // falhar (site mais antigo, ou o utilizador ja colou diretamente
            // o URL da API), fica-se com o URL original tal como veio.
            var apiBaseUrl = await remoteClient.ResolveApiBaseAsync(baseUrl, ct);

            // So e no-op garantido se o ambiente remoto resolvido e este
            // mesmo host (nao so o mesmo email - o mesmo utilizador tem
            // legitimamente contas com o mesmo email em ambientes diferentes,
            // e essa e precisamente a razao de existir esta funcionalidade).
            var callerEmail = caller.FindFirstValue(ClaimTypes.Email) ?? caller.FindFirstValue(ClaimTypes.Name);
            var isSameHost = string.Equals(apiBaseUrl.Authority, httpContext.Request.Host.ToUriComponent(), StringComparison.OrdinalIgnoreCase);
            if (isSameHost && !string.IsNullOrEmpty(callerEmail) && string.Equals(callerEmail, request.Email, StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest(new { error = "Não pode importar veículos de si mesmo — indique as credenciais de outro utilizador." });

            var token = await remoteClient.LoginAsync(apiBaseUrl, request.Email, request.Password, ct);

            Guid remoteHouseholdId;
            if (request.RemoteHouseholdId is { } chosen)
            {
                remoteHouseholdId = chosen;
            }
            else
            {
                var households = await remoteClient.GetHouseholdsAsync(apiBaseUrl, token, ct);
                if (households.Count == 0)
                    return Results.BadRequest(new { error = "O utilizador de origem não pertence a nenhum household." });

                if (households.Count > 1)
                    return Results.Ok(new ImportPreviewResponse(ImportPreviewStatus.ChooseHousehold, households, []));

                remoteHouseholdId = households[0].Id;
            }

            var remoteVehicles = await remoteClient.GetVehiclesAsync(apiBaseUrl, remoteHouseholdId, token, ct);

            var localPlates = await db.Vehicles
                .Where(v => v.HouseholdId == householdId)
                .Select(v => v.Plate)
                .ToListAsync(ct);
            var localPlateSet = new HashSet<string>(localPlates, StringComparer.OrdinalIgnoreCase);

            var items = remoteVehicles
                .Select(v => new ImportPreviewItem(v, localPlateSet.Contains(v.Plate)))
                .ToList();

            return Results.Ok(new ImportPreviewResponse(ImportPreviewStatus.VehiclesReady, [], items));
        }
        catch (ImportRemoteException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    public static async Task<IResult> ConfirmImport(Guid householdId, ImportConfirmRequest request, AutoDbContext db, IEventPublisher events, CancellationToken ct)
    {
        var localPlates = await db.Vehicles
            .Where(v => v.HouseholdId == householdId)
            .Select(v => v.Plate)
            .ToListAsync(ct);
        var seenPlates = new HashSet<string>(localPlates, StringComparer.OrdinalIgnoreCase);

        var results = new List<ImportResultItem>();
        foreach (var candidate in request.Vehicles)
        {
            if (!seenPlates.Add(candidate.Plate))
            {
                results.Add(new ImportResultItem(candidate.Plate, false, "Matrícula já existe"));
                continue;
            }

            var upsertRequest = new VehicleUpsertRequest(
                candidate.Category, candidate.Brand, candidate.Model, candidate.Plate, candidate.Vin,
                candidate.Color, candidate.Driver, candidate.Odometer, candidate.Registered, candidate.NextInspection,
                candidate.Insurer, candidate.InsurancePolicyNumber, candidate.InsurancePeriodStart,
                candidate.InsurancePeriodEnd, candidate.InsurancePremium, candidate.IucDueDate);

            try
            {
                await CreateVehicleEntityAsync(householdId, upsertRequest, db, events, ct);
                results.Add(new ImportResultItem(candidate.Plate, true, null));
            }
            catch (ArgumentException ex)
            {
                // Um veiculo invalido (ex.: sem cor, um campo que se
                // tornou obrigatorio depois de outro ambiente ja o ter
                // criado sem ele) nao deve parar o resto do lote.
                results.Add(new ImportResultItem(candidate.Plate, false, ex.Message));
            }
        }

        var importedCount = results.Count(r => r.Imported);
        return Results.Ok(new ImportConfirmResponse(importedCount, results.Count - importedCount, results));
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
        var maintenance = await CreateMaintenanceEntityAsync(vehicleId, request, db, ct);
        return maintenance is null
            ? Results.NotFound()
            : Results.Created($"/api/auto/maintenances/{maintenance.Id}", MaintenanceResponse.From(maintenance));
    }

    /// Partilhada com AutoMcpTools.CreateMaintenance.
    internal static async Task<Maintenance?> CreateMaintenanceEntityAsync(
        Guid vehicleId, MaintenanceUpsertRequest request, AutoDbContext db, CancellationToken ct)
    {
        if (!await db.Vehicles.AnyAsync(v => v.Id == vehicleId, ct))
            return null;

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

        return maintenance;
    }

    public static async Task<IResult> GetMaintenance(Guid id, AutoDbContext db, CancellationToken ct) =>
        await db.Maintenances.Include(m => m.Items).FirstOrDefaultAsync(m => m.Id == id, ct) is { } m
            ? Results.Ok(MaintenanceResponse.From(m))
            : Results.NotFound();

    public static async Task<IResult> UpdateMaintenance(
        Guid id, MaintenanceUpsertRequest request, AutoDbContext db, CancellationToken ct)
    {
        var maintenance = await UpdateMaintenanceEntityAsync(id, request, db, ct);
        return maintenance is null ? Results.NotFound() : Results.Ok(MaintenanceResponse.From(maintenance));
    }

    /// Partilhada com AutoMcpTools.UpdateMaintenance.
    internal static async Task<Maintenance?> UpdateMaintenanceEntityAsync(
        Guid id, MaintenanceUpsertRequest request, AutoDbContext db, CancellationToken ct)
    {
        var maintenance = await db.Maintenances.Include(m => m.Items).FirstOrDefaultAsync(m => m.Id == id, ct);
        if (maintenance is null)
            return null;

        maintenance.Date = request.Date;
        maintenance.Odometer = request.Odometer;
        maintenance.Workshop = request.Workshop;
        maintenance.Description = request.Description;
        maintenance.Type = request.Type;
        maintenance.Cost = request.Cost;
        maintenance.InvoiceNumber = request.InvoiceNumber;
        maintenance.InvoiceDate = request.InvoiceDate;

        await db.SaveChangesAsync(ct);
        return maintenance;
    }

    public static async Task<IResult> DeleteMaintenance(Guid id, AutoDbContext db, CancellationToken ct) =>
        await DeleteMaintenanceEntityAsync(id, db, ct) ? Results.NoContent() : Results.NotFound();

    /// Partilhada com AutoMcpTools.DeleteMaintenance.
    internal static async Task<bool> DeleteMaintenanceEntityAsync(Guid id, AutoDbContext db, CancellationToken ct)
    {
        var maintenance = await db.Maintenances.FindAsync([id], ct);
        if (maintenance is null)
            return false;

        db.Maintenances.Remove(maintenance);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public static IResult CreateUploadUrl(Guid vehicleId, UploadUrlRequest request, IObjectStorage storage) =>
        Results.Ok(CreateUploadUrlResponse(vehicleId, request, storage));

    /// Partilhada com AutoMcpTools.CreateDocumentUploadUrl.
    internal static UploadUrlResponse CreateUploadUrlResponse(Guid vehicleId, UploadUrlRequest request, IObjectStorage storage)
    {
        var objectKey = $"vehicles/{vehicleId}/{Guid.NewGuid()}-{request.FileName}";
        var (uploadUrl, expiresAt) = storage.CreateUploadUrl(objectKey, request.ContentType);
        return new UploadUrlResponse(objectKey, uploadUrl, expiresAt);
    }

    public static async Task<IResult> CreateDocument(
        Guid vehicleId, DocumentCreateRequest request, AutoDbContext db, CancellationToken ct)
    {
        var document = await CreateDocumentEntityAsync(vehicleId, request, db, ct);
        return document is null
            ? Results.NotFound()
            : Results.Created($"/api/auto/documents/{document.Id}", DocumentResponse.From(document));
    }

    /// Partilhada com AutoMcpTools.CreateDocument.
    internal static async Task<VehicleDocument?> CreateDocumentEntityAsync(
        Guid vehicleId, DocumentCreateRequest request, AutoDbContext db, CancellationToken ct)
    {
        if (!await db.Vehicles.AnyAsync(v => v.Id == vehicleId, ct))
            return null;

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

        return document;
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

    public static async Task<IResult> DeleteDocument(Guid id, AutoDbContext db, CancellationToken ct) =>
        await DeleteDocumentEntityAsync(id, db, ct) ? Results.NoContent() : Results.NotFound();

    /// Partilhada com AutoMcpTools.DeleteDocument.
    internal static async Task<bool> DeleteDocumentEntityAsync(Guid id, AutoDbContext db, CancellationToken ct)
    {
        var document = await db.Documents.FindAsync([id], ct);
        if (document is null)
            return false;

        db.Documents.Remove(document);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public static async Task<IResult> GetStats(Guid vehicleId, AutoDbContext db, CancellationToken ct) =>
        Results.Ok(await GetStatsAsync(vehicleId, db, ct));

    /// Partilhada com AutoMcpTools.GetVehicleStats.
    internal static async Task<VehicleStatsResponse> GetStatsAsync(Guid vehicleId, AutoDbContext db, CancellationToken ct)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));

        var recent = await db.Maintenances
            .Where(m => m.VehicleId == vehicleId && m.Date >= since)
            .ToListAsync(ct);

        var vehicle = await db.Vehicles.FindAsync([vehicleId], ct);
        var kmsLastMonth = recent.Count > 0 ? Math.Max(0, (vehicle?.Odometer ?? 0) - recent.Min(m => m.Odometer)) : 0;

        return new VehicleStatsResponse(
            kmsLastMonth,
            recent.Sum(m => m.Cost),
            recent.Count);
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
