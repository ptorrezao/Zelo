using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Endpoints;
using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto.Tests;

public class AutoEndpointHandlersTests
{
    private static AutoDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AutoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static VehicleUpsertRequest NewVehicleRequest() => new(
        VehicleCategory.Ligeiros, "Toyota", "Corolla", "AA-00-BB", "VIN123",
        "Pedro", 10_000, new DateOnly(2020, 1, 1), null, "Fidelidade", null, null);

    [Fact]
    public async Task CreateVehicle_PersistsAndPublishesCreatedEvent()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var householdId = Guid.NewGuid();

        var result = await AutoEndpointHandlers.CreateVehicle(householdId, NewVehicleRequest(), db, events, CancellationToken.None);

        var created = Assert.IsType<Created<VehicleResponse>>(result);
        Assert.Equal("Toyota", created.Value!.Brand);
        Assert.Equal(1, await db.Vehicles.CountAsync());
        Assert.Single(events.Published);
    }

    [Fact]
    public async Task CreateVehicle_ComProximaInspecao_TambemPublicaObligationScheduled()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var request = NewVehicleRequest() with { NextInspection = new DateOnly(2027, 6, 1) };

        await AutoEndpointHandlers.CreateVehicle(Guid.NewGuid(), request, db, events, CancellationToken.None);

        Assert.Equal(2, events.Published.Count);
        var vehicle = await db.Vehicles.FirstAsync();
        Assert.NotNull(vehicle.InspectionObligationId);
    }

    [Fact]
    public async Task GetVehicles_FiltraPorHousehold_OrdenaPorMarcaEModelo()
    {
        await using var db = NewDb();
        var household = Guid.NewGuid();
        var outro = Guid.NewGuid();
        db.Vehicles.AddRange(
            NewVehicle(household, "Volvo", "XC60"),
            NewVehicle(household, "Audi", "A3"),
            NewVehicle(outro, "BMW", "X1"));
        await db.SaveChangesAsync();

        var result = await AutoEndpointHandlers.GetVehicles(household, db, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Audi", result[0].Brand);
        Assert.Equal("Volvo", result[1].Brand);
    }

    [Fact]
    public async Task GetVehicle_Inexistente_DevolveNotFound()
    {
        await using var db = NewDb();

        var result = await AutoEndpointHandlers.GetVehicle(Guid.NewGuid(), db, CancellationToken.None);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task UpdateVehicle_Existente_AtualizaCampos()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var vehicle = NewVehicle(Guid.NewGuid(), "Toyota", "Corolla");
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        var request = NewVehicleRequest() with { Odometer = 55_000 };

        var result = await AutoEndpointHandlers.UpdateVehicle(vehicle.Id, request, db, events, CancellationToken.None);

        var ok = Assert.IsType<Ok<VehicleResponse>>(result);
        Assert.Equal(55_000, ok.Value!.Odometer);
    }

    [Fact]
    public async Task UpdateVehicle_Inexistente_DevolveNotFound()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();

        var result = await AutoEndpointHandlers.UpdateVehicle(Guid.NewGuid(), NewVehicleRequest(), db, events, CancellationToken.None);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task DeleteVehicle_MarcaStatusEPublicaArchived()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var vehicle = NewVehicle(Guid.NewGuid(), "Toyota", "Corolla");
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var result = await AutoEndpointHandlers.DeleteVehicle(vehicle.Id, VehicleStatus.Vendido, db, events, CancellationToken.None);

        Assert.IsType<NoContent>(result);
        Assert.Equal(VehicleStatus.Vendido, (await db.Vehicles.FindAsync(vehicle.Id))!.Status);
        Assert.Single(events.Published);
    }

    [Fact]
    public async Task CreateMaintenance_VeiculoInexistente_DevolveNotFound()
    {
        await using var db = NewDb();

        var result = await AutoEndpointHandlers.CreateMaintenance(Guid.NewGuid(), NewMaintenanceRequest(), db, CancellationToken.None);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task CreateMaintenance_ComItens_PersisteMaintenanceEItems()
    {
        await using var db = NewDb();
        var vehicle = NewVehicle(Guid.NewGuid(), "Toyota", "Corolla");
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        var request = NewMaintenanceRequest() with
        {
            Items = [new MaintenanceItemRequest("Oleo", 45.5m, "SN-1")],
        };

        var result = await AutoEndpointHandlers.CreateMaintenance(vehicle.Id, request, db, CancellationToken.None);

        var created = Assert.IsType<Created<MaintenanceResponse>>(result);
        Assert.Single(created.Value!.Items);
        Assert.Equal("Oleo", created.Value.Items[0].Description);
    }

    [Fact]
    public async Task GetMaintenances_OrdenaPorDataDescendente()
    {
        await using var db = NewDb();
        var vehicleId = Guid.NewGuid();
        db.Maintenances.AddRange(
            NewMaintenance(vehicleId, new DateOnly(2026, 1, 1)),
            NewMaintenance(vehicleId, new DateOnly(2026, 6, 1)));
        await db.SaveChangesAsync();

        var result = await AutoEndpointHandlers.GetMaintenances(vehicleId, db, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(new DateOnly(2026, 6, 1), result[0].Date);
    }

    [Fact]
    public async Task UpdateMaintenance_Inexistente_DevolveNotFound()
    {
        await using var db = NewDb();

        var result = await AutoEndpointHandlers.UpdateMaintenance(Guid.NewGuid(), NewMaintenanceRequest(), db, CancellationToken.None);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task DeleteMaintenance_Existente_Remove()
    {
        await using var db = NewDb();
        var maintenance = NewMaintenance(Guid.NewGuid(), new DateOnly(2026, 1, 1));
        db.Maintenances.Add(maintenance);
        await db.SaveChangesAsync();

        var result = await AutoEndpointHandlers.DeleteMaintenance(maintenance.Id, db, CancellationToken.None);

        Assert.IsType<NoContent>(result);
        Assert.Equal(0, await db.Maintenances.CountAsync());
    }

    [Fact]
    public void CreateUploadUrl_DevolveObjectKeyComPrefixoDoVeiculo()
    {
        var vehicleId = Guid.NewGuid();
        var storage = new FakeObjectStorage();

        var result = AutoEndpointHandlers.CreateUploadUrl(vehicleId, new UploadUrlRequest("fatura.pdf", "application/pdf"), storage);

        var ok = Assert.IsType<Ok<UploadUrlResponse>>(result);
        Assert.StartsWith($"vehicles/{vehicleId}/", ok.Value!.ObjectKey);
        Assert.EndsWith("-fatura.pdf", ok.Value.ObjectKey);
    }

    [Fact]
    public async Task CreateDocument_VeiculoInexistente_DevolveNotFound()
    {
        await using var db = NewDb();

        var result = await AutoEndpointHandlers.CreateDocument(Guid.NewGuid(), NewDocumentRequest(), db, CancellationToken.None);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task CreateDocument_VeiculoExistente_Persiste()
    {
        await using var db = NewDb();
        var vehicle = NewVehicle(Guid.NewGuid(), "Toyota", "Corolla");
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var result = await AutoEndpointHandlers.CreateDocument(vehicle.Id, NewDocumentRequest(), db, CancellationToken.None);

        var created = Assert.IsType<Created<DocumentResponse>>(result);
        Assert.Equal("Apolice.pdf", created.Value!.Name);
    }

    [Fact]
    public async Task GetDocuments_FiltraPorCategoriaQuandoIndicada()
    {
        await using var db = NewDb();
        var vehicleId = Guid.NewGuid();
        db.Documents.AddRange(
            NewDocument(vehicleId, DocumentCategory.Seguro),
            NewDocument(vehicleId, DocumentCategory.Manutencao));
        await db.SaveChangesAsync();

        var onlyInsurance = await AutoEndpointHandlers.GetDocuments(vehicleId, DocumentCategory.Seguro, db, CancellationToken.None);
        var all = await AutoEndpointHandlers.GetDocuments(vehicleId, null, db, CancellationToken.None);

        Assert.Single(onlyInsurance);
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task DeleteDocument_Inexistente_DevolveNotFound()
    {
        await using var db = NewDb();

        var result = await AutoEndpointHandlers.DeleteDocument(Guid.NewGuid(), db, CancellationToken.None);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task GetStats_SemManutencoesRecentes_DevolveZeros()
    {
        await using var db = NewDb();
        var vehicle = NewVehicle(Guid.NewGuid(), "Toyota", "Corolla");
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var result = await AutoEndpointHandlers.GetStats(vehicle.Id, db, CancellationToken.None);

        var ok = Assert.IsType<Ok<VehicleStatsResponse>>(result);
        Assert.Equal(0, ok.Value!.MaintenanceCountLastMonth);
        Assert.Equal(0, ok.Value.MaintenanceCostLastMonth);
    }

    [Fact]
    public async Task GetStats_ComManutencoesRecentes_SomaCustosEContagem()
    {
        await using var db = NewDb();
        var vehicle = NewVehicle(Guid.NewGuid(), "Toyota", "Corolla");
        vehicle.Odometer = 12_000;
        db.Vehicles.Add(vehicle);
        var recentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        db.Maintenances.Add(new Maintenance
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Date = recentDate,
            Odometer = 11_500,
            Workshop = "Oficina X",
            Description = "Revisao",
            Type = MaintenanceType.Preventiva,
            Cost = 120m,
        });
        await db.SaveChangesAsync();

        var result = await AutoEndpointHandlers.GetStats(vehicle.Id, db, CancellationToken.None);

        var ok = Assert.IsType<Ok<VehicleStatsResponse>>(result);
        Assert.Equal(1, ok.Value!.MaintenanceCountLastMonth);
        Assert.Equal(120m, ok.Value.MaintenanceCostLastMonth);
        Assert.Equal(500, ok.Value.KmsLastMonth);
    }

    private static Vehicle NewVehicle(Guid householdId, string brand, string model) => new()
    {
        Id = Guid.NewGuid(),
        HouseholdId = householdId,
        Category = VehicleCategory.Ligeiros,
        Brand = brand,
        Model = model,
        Plate = "AA-00-BB",
        Vin = Guid.NewGuid().ToString("N"),
        Registered = new DateOnly(2020, 1, 1),
    };

    private static Maintenance NewMaintenance(Guid vehicleId, DateOnly date) => new()
    {
        Id = Guid.NewGuid(),
        VehicleId = vehicleId,
        Date = date,
        Odometer = 1000,
        Workshop = "Oficina",
        Description = "Revisao",
        Type = MaintenanceType.Preventiva,
        Cost = 50m,
    };

    private static MaintenanceUpsertRequest NewMaintenanceRequest() => new(
        new DateOnly(2026, 1, 1), 1000, "Oficina", "Revisao", MaintenanceType.Preventiva, 50m, null, null, null);

    private static VehicleDocument NewDocument(Guid vehicleId, DocumentCategory category) => new()
    {
        Id = Guid.NewGuid(),
        VehicleId = vehicleId,
        Name = "Doc.pdf",
        Category = category,
        Type = DocumentType.Pdf,
        Date = new DateOnly(2026, 1, 1),
        SizeBytes = 1024,
        ObjectKey = "vehicles/x/doc.pdf",
    };

    private static DocumentCreateRequest NewDocumentRequest() => new(
        "vehicles/x/apolice.pdf", "Apolice.pdf", DocumentCategory.Seguro, DocumentType.Pdf, new DateOnly(2026, 1, 1), 2048);
}
