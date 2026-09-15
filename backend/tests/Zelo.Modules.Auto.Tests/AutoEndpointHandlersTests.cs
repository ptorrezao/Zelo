using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Zelo.Contracts;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Endpoints;
using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto.Tests;

public class AutoEndpointHandlersTests
{
    private static AutoDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AutoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static VehicleUpsertRequest NewVehicleRequest() => new(
        VehicleCategory.Ligeiros, "Toyota", "Corolla", "AA-00-BB", "VIN123", "Branco",
        "Pedro", 10_000, new DateOnly(2020, 1, 1), null, "Fidelidade", "AP-12345",
        new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1), 350.00m, null);

    [Fact]
    public void GetVehicleCatalog_DevolveMarcasEModelosPorCategoria()
    {
        var result = AutoEndpointHandlers.GetVehicleCatalog();

        var ok = Assert.IsType<Ok<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string[]>>>>(result);
        Assert.True(ok.Value!.Count > 0);
        Assert.Contains("Ligeiros", ok.Value.Keys);
        Assert.Contains("Toyota", ok.Value["Ligeiros"].Keys);
        Assert.Contains("Corolla", ok.Value["Ligeiros"]["Toyota"]);
        // BMW e Honda tem modelos diferentes em cada categoria - a mesma
        // marca nao pode sugerir motas quando a categoria escolhida e
        // Ligeiros, nem o contrario.
        Assert.Contains("Motociclos", ok.Value.Keys);
        Assert.Contains("R1250GS", ok.Value["Motociclos"]["BMW"]);
        Assert.DoesNotContain("R1250GS", ok.Value["Ligeiros"]["BMW"]);
    }

    [Fact]
    public async Task CreateVehicle_PersistsAndPublishesCreatedEvent()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var householdId = Guid.NewGuid();

        var result = await AutoEndpointHandlers.CreateVehicle(householdId, NewVehicleRequest(), db, events, new FakeObjectStorage(), CancellationToken.None);

        var created = Assert.IsType<Created<VehicleResponse>>(result);
        Assert.Equal("Toyota", created.Value!.Brand);
        Assert.Equal("Branco", created.Value.Color);
        Assert.Equal("AP-12345", created.Value.InsurancePolicyNumber);
        Assert.Equal(350.00m, created.Value.InsurancePremium);
        Assert.Equal(1, await db.Vehicles.CountAsync());
        Assert.Single(events.Published);
    }

    [Fact]
    public async Task UpdateVehicle_MudaCor()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var vehicle = NewVehicle(Guid.NewGuid(), "Toyota", "Corolla");
        vehicle.Color = "Branco";
        vehicle.PhotoObjectKey = "vehicles/x/photo.png"; // gerada na criacao, para a cor antiga
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        var request = NewVehicleRequest() with { Color = "Azul" };

        var result = await AutoEndpointHandlers.UpdateVehicle(vehicle.Id, request, db, events, new FakeObjectStorage(), CancellationToken.None);

        var ok = Assert.IsType<Ok<VehicleResponse>>(result);
        Assert.Equal("Azul", ok.Value!.Color);
        // A foto gerada e para a cor antiga - apaga a referencia e volta a
        // publicar AssetCreated para o VehiclePhotoHandler gerar de novo.
        Assert.Null((await db.Vehicles.FindAsync(vehicle.Id))!.PhotoObjectKey);
        Assert.Contains(events.Published, e => e is AssetCreated created && created.AssetId == vehicle.Id);
    }

    [Fact]
    public async Task UpdateVehicle_CorInalterada_NaoRepublicaAssetCreated()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var vehicle = NewVehicle(Guid.NewGuid(), "Toyota", "Corolla");
        vehicle.Color = "Branco";
        vehicle.PhotoObjectKey = "vehicles/x/photo.png";
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        var request = NewVehicleRequest() with { Color = "Branco", Odometer = 20_000 };

        await AutoEndpointHandlers.UpdateVehicle(vehicle.Id, request, db, events, new FakeObjectStorage(), CancellationToken.None);

        Assert.Equal("vehicles/x/photo.png", (await db.Vehicles.FindAsync(vehicle.Id))!.PhotoObjectKey);
        Assert.DoesNotContain(events.Published, e => e is AssetCreated);
    }

    [Fact]
    public async Task CreateVehicle_ComProximaInspecao_TambemPublicaObligationScheduled()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var request = NewVehicleRequest() with { NextInspection = new DateOnly(2027, 6, 1) };

        await AutoEndpointHandlers.CreateVehicle(Guid.NewGuid(), request, db, events, new FakeObjectStorage(), CancellationToken.None);

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

        var result = await AutoEndpointHandlers.GetVehicles(household, db, new FakeObjectStorage(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Audi", result[0].Brand);
        Assert.Equal("Volvo", result[1].Brand);
    }

    [Fact]
    public async Task GetVehicle_Inexistente_DevolveNotFound()
    {
        await using var db = NewDb();

        var result = await AutoEndpointHandlers.GetVehicle(Guid.NewGuid(), db, new FakeObjectStorage(), CancellationToken.None);

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

        var result = await AutoEndpointHandlers.UpdateVehicle(vehicle.Id, request, db, events, new FakeObjectStorage(), CancellationToken.None);

        var ok = Assert.IsType<Ok<VehicleResponse>>(result);
        Assert.Equal(55_000, ok.Value!.Odometer);
        Assert.Equal("Branco", ok.Value.Color);
    }

    [Fact]
    public async Task UpdateVehicle_Inexistente_DevolveNotFound()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();

        var result = await AutoEndpointHandlers.UpdateVehicle(Guid.NewGuid(), NewVehicleRequest(), db, events, new FakeObjectStorage(), CancellationToken.None);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task CreateVehicle_SemCor_DevolveBadRequestSemGuardarNada()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var request = NewVehicleRequest() with { Color = null };

        var result = await AutoEndpointHandlers.CreateVehicle(Guid.NewGuid(), request, db, events, new FakeObjectStorage(), CancellationToken.None);

        Assert.Contains("Cor", GetErrorMessage(result));
        Assert.Equal(0, await db.Vehicles.CountAsync());
        Assert.Empty(events.Published);
    }

    [Fact]
    public async Task UpdateVehicle_QuilometragemNegativa_DevolveBadRequestSemAlterarNada()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var vehicle = NewVehicle(Guid.NewGuid(), "Toyota", "Corolla");
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        var request = NewVehicleRequest() with { Odometer = -1 };

        var result = await AutoEndpointHandlers.UpdateVehicle(vehicle.Id, request, db, events, new FakeObjectStorage(), CancellationToken.None);

        Assert.Contains("Quilometragem", GetErrorMessage(result));
        Assert.Equal(0, (await db.Vehicles.FindAsync(vehicle.Id))!.Odometer); // NewVehicle por omissao
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

    private static ImportCandidateVehicle NewCandidate(string plate) => new(
        VehicleCategory.Ligeiros, "Toyota", "Corolla", plate, "VIN123", "Branco",
        "Pedro", 10_000, new DateOnly(2020, 1, 1), null, "Fidelidade", "AP-12345",
        new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1), 350.00m, null);

    private static ClaimsPrincipal CallerWithEmail(string email) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Email, email)]));

    /// PreviewImport agora precisa de um HttpContext para comparar o host do
    /// pedido com o do ambiente remoto resolvido (ver
    /// plans/fix-cross-environment-import-false-self-block.md).
    private static HttpContext HttpContextWithHost(string host)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Host = new HostString(host);
        return ctx;
    }

    private static string GetErrorMessage(IResult result)
    {
        var value = Assert.IsAssignableFrom<IValueHttpResult>(result);
        return (string)value.Value!.GetType().GetProperty("error")!.GetValue(value.Value)!;
    }

    [Fact]
    public async Task PreviewImport_MarcaVeiculosComMatriculaJaExistenteComoAlreadyExists()
    {
        await using var db = NewDb();
        var destino = Guid.NewGuid();
        db.Vehicles.Add(NewVehicle(destino, "Volvo", "XC60")); // Plate = "AA-00-BB" por omissao
        await db.SaveChangesAsync();
        var remoteClient = new FakeImportRemoteClient
        {
            Vehicles = [NewCandidate("AA-00-BB"), NewCandidate("CC-11-DD")],
        };
        var request = new ImportConnectRequest("https://origem.exemplo", "user@origem.com", "pwd", Guid.NewGuid());

        var result = await AutoEndpointHandlers.PreviewImport(
            destino, request, CallerWithEmail("destino@exemplo.com"),
            HttpContextWithHost("destino.exemplo"), db, remoteClient, CancellationToken.None);

        var ok = Assert.IsType<Ok<ImportPreviewResponse>>(result);
        Assert.Equal(ImportPreviewStatus.VehiclesReady, ok.Value!.Status);
        Assert.True(ok.Value.Vehicles.Single(i => i.Vehicle.Plate == "AA-00-BB").AlreadyExists);
        Assert.False(ok.Value.Vehicles.Single(i => i.Vehicle.Plate == "CC-11-DD").AlreadyExists);
    }

    [Fact]
    public async Task PreviewImport_SemRemoteHouseholdId_ComMultiplosHouseholds_PedeEscolha()
    {
        await using var db = NewDb();
        var remoteClient = new FakeImportRemoteClient
        {
            Households = [new ImportHouseholdOption(Guid.NewGuid(), "Casa 1"), new ImportHouseholdOption(Guid.NewGuid(), "Casa 2")],
        };
        var request = new ImportConnectRequest("https://origem.exemplo", "user@origem.com", "pwd", null);

        var result = await AutoEndpointHandlers.PreviewImport(
            Guid.NewGuid(), request, CallerWithEmail("destino@exemplo.com"),
            HttpContextWithHost("destino.exemplo"), db, remoteClient, CancellationToken.None);

        var ok = Assert.IsType<Ok<ImportPreviewResponse>>(result);
        Assert.Equal(ImportPreviewStatus.ChooseHousehold, ok.Value!.Status);
        Assert.Equal(2, ok.Value.Households.Count);
        Assert.Empty(ok.Value.Vehicles);
    }

    [Fact]
    public async Task PreviewImport_FalhaNoRemoto_DevolveBadRequestComMensagem()
    {
        await using var db = NewDb();
        var remoteClient = new FakeImportRemoteClient
        {
            FailWith = new ImportRemoteException("Não foi possível autenticar no ambiente de origem."),
        };
        var request = new ImportConnectRequest("https://origem.exemplo", "user@origem.com", "pwd", Guid.NewGuid());

        var result = await AutoEndpointHandlers.PreviewImport(
            Guid.NewGuid(), request, CallerWithEmail("destino@exemplo.com"),
            HttpContextWithHost("destino.exemplo"), db, remoteClient, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, statusResult.StatusCode);
    }

    [Fact]
    public async Task PreviewImport_MesmoEmailEMesmoHost_BloqueiaAntesDoLogin()
    {
        await using var db = NewDb();
        // ResolveApiBaseAsync (fake) faz eco do BaseUrl - mesmo host que o
        // pedido esta a chegar => e garantidamente o proprio ambiente.
        var remoteClient = new FakeImportRemoteClient();
        var request = new ImportConnectRequest("https://origem.exemplo", "eu@exemplo.com", "pwd", Guid.NewGuid());

        var result = await AutoEndpointHandlers.PreviewImport(
            Guid.NewGuid(), request, CallerWithEmail("eu@exemplo.com"),
            HttpContextWithHost("origem.exemplo"), db, remoteClient, CancellationToken.None);

        Assert.Contains("si mesmo", GetErrorMessage(result));
    }

    [Fact]
    public async Task PreviewImport_MesmoEmailMasHostDiferente_NaoBloqueiaSeguePorLogin()
    {
        await using var db = NewDb();
        // Mesmo email de login dos dois lados, mas ambientes (hosts)
        // diferentes - caso normal de importacao entre ambientes, nao deve
        // bloquear (ver plans/fix-cross-environment-import-false-self-block.md).
        var remoteClient = new FakeImportRemoteClient
        {
            Households = [new ImportHouseholdOption(Guid.NewGuid(), "Casa 1")],
            Vehicles = [NewCandidate("CC-11-DD")],
        };
        var request = new ImportConnectRequest("https://origem.exemplo", "eu@exemplo.com", "pwd", null);

        var result = await AutoEndpointHandlers.PreviewImport(
            Guid.NewGuid(), request, CallerWithEmail("eu@exemplo.com"),
            HttpContextWithHost("destino.exemplo"), db, remoteClient, CancellationToken.None);

        var ok = Assert.IsType<Ok<ImportPreviewResponse>>(result);
        Assert.Equal(ImportPreviewStatus.VehiclesReady, ok.Value!.Status);
    }

    [Fact]
    public async Task ConfirmImport_IgnoraDuplicadosContraBdEDentroDoLote_ImportaOResto()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var destino = Guid.NewGuid();
        db.Vehicles.Add(NewVehicle(destino, "Volvo", "XC60")); // Plate = "AA-00-BB" por omissao
        await db.SaveChangesAsync();
        var request = new ImportConfirmRequest([
            NewCandidate("AA-00-BB"), // ja existe na BD
            NewCandidate("CC-11-DD"),
            NewCandidate("CC-11-DD"), // duplicado dentro do proprio lote
        ]);

        var result = await AutoEndpointHandlers.ConfirmImport(destino, request, db, events, CancellationToken.None);

        var ok = Assert.IsType<Ok<ImportConfirmResponse>>(result);
        Assert.Equal(1, ok.Value!.ImportedCount);
        Assert.Equal(2, ok.Value.SkippedCount);
        Assert.Equal(2, await db.Vehicles.CountAsync(v => v.HouseholdId == destino));
    }

    [Fact]
    public async Task ConfirmImport_CandidatoInvalido_MarcaFalhaSemPararOLote()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var destino = Guid.NewGuid();
        var request = new ImportConfirmRequest([
            NewCandidate("AA-00-BB") with { Color = null }, // invalido - sem cor
            NewCandidate("CC-11-DD"), // valido, nao pode ser afetado pelo anterior
        ]);

        var result = await AutoEndpointHandlers.ConfirmImport(destino, request, db, events, CancellationToken.None);

        var ok = Assert.IsType<Ok<ImportConfirmResponse>>(result);
        Assert.Equal(1, ok.Value!.ImportedCount);
        Assert.Equal(1, ok.Value.SkippedCount);
        var failed = ok.Value.Items.Single(r => r.Plate == "AA-00-BB");
        Assert.False(failed.Imported);
        Assert.Contains("Cor", failed.SkipReason);
        Assert.True(ok.Value.Items.Single(r => r.Plate == "CC-11-DD").Imported);
        Assert.Equal(1, await db.Vehicles.CountAsync(v => v.HouseholdId == destino));
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
