using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol;
using Xunit;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Endpoints;
using Zelo.Modules.Auto.Infrastructure;
using Zelo.SharedKernel;

namespace Zelo.Modules.Auto.Tests;

public class AutoMcpToolsTests
{
    private static AutoDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AutoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static IHttpContextAccessor NewHttpContextAccessor(Guid? userId = null)
    {
        var context = new DefaultHttpContext();
        if (userId is { } id)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, id.ToString())], "test"));
        }

        return new FakeHttpContextAccessor(context);
    }

    private sealed class FakeHttpContextAccessor(HttpContext context) : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = context;
    }

    /// Membership permissiva ou restritiva de acordo com o Guid pedido -
    /// simples o suficiente para nao precisar de mocking framework.
    private sealed class FakeMembershipChecker(
        bool isMember = true,
        IReadOnlyList<HouseholdSummary>? households = null,
        HouseholdSummary? renameResult = null,
        Exception? renameThrows = null,
        Exception? createThrows = null) : IHouseholdMembershipChecker
    {
        public Task<bool> IsMemberAsync(Guid userId, Guid householdId, CancellationToken ct = default) =>
            Task.FromResult(isMember);

        public Task<IReadOnlyList<HouseholdSummary>> GetMyHouseholdsAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult(households ?? []);

        public Task<HouseholdSummary> CreateHouseholdAsync(Guid userId, string name, CancellationToken ct = default) =>
            createThrows is null ? Task.FromResult(new HouseholdSummary(Guid.NewGuid(), name, false)) : throw createThrows;

        public Task<HouseholdSummary?> RenameHouseholdAsync(Guid userId, Guid householdId, string name, CancellationToken ct = default) =>
            renameThrows is null ? Task.FromResult(renameResult) : throw renameThrows;
    }

    private static Vehicle NewVehicle(Guid householdId, string brand = "Toyota", string model = "Corolla") => new()
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

    [Fact]
    public async Task ListHouseholds_DevolveOsHouseholdsDoUtilizadorAutenticado()
    {
        var households = new List<HouseholdSummary> { new(Guid.NewGuid(), "A minha casa", true) };

        var result = await AutoMcpTools.ListHouseholds(
            new FakeMembershipChecker(households: households), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None);

        Assert.Same(households, result);
    }

    [Fact]
    public async Task ListHouseholds_SemUtilizadorAutenticado_LancaMcpException()
    {
        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.ListHouseholds(
            new FakeMembershipChecker(), NewHttpContextAccessor(), CancellationToken.None));
    }

    [Fact]
    public async Task CreateHousehold_Sucesso_DevolveOHouseholdCriado()
    {
        var result = await AutoMcpTools.CreateHousehold(
            "Casa de férias", new FakeMembershipChecker(), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal("Casa de férias", result.Name);
    }

    [Fact]
    public async Task CreateHousehold_NomeInvalido_LancaMcpException()
    {
        var checker = new FakeMembershipChecker(createThrows: new ArgumentException("Nome do household inválido (entre 1 e 200 caracteres)."));

        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.CreateHousehold(
            "", checker, NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task RenameHousehold_Sucesso_DevolveOHouseholdRenomeado()
    {
        var householdId = Guid.NewGuid();
        var checker = new FakeMembershipChecker(renameResult: new HouseholdSummary(householdId, "Novo nome", false));

        var result = await AutoMcpTools.RenameHousehold(
            householdId, "Novo nome", checker, NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal("Novo nome", result.Name);
    }

    [Fact]
    public async Task RenameHousehold_NaoEncontrado_LancaMcpException()
    {
        var checker = new FakeMembershipChecker(renameResult: null);

        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.RenameHousehold(
            Guid.NewGuid(), "Novo nome", checker, NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task RenameHousehold_UtilizadorNaoEOwner_LancaMcpException()
    {
        var checker = new FakeMembershipChecker(renameThrows: new UnauthorizedAccessException("Só o Owner pode renomear este household."));

        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.RenameHousehold(
            Guid.NewGuid(), "Novo nome", checker, NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task ListVehicles_UtilizadorMembro_DevolveVeiculosDoHousehold()
    {
        await using var db = NewDb();
        var householdId = Guid.NewGuid();
        db.Vehicles.Add(NewVehicle(householdId));
        db.Vehicles.Add(NewVehicle(Guid.NewGuid())); // outro household
        await db.SaveChangesAsync();

        var result = await AutoMcpTools.ListVehicles(
            householdId, db, new FakeObjectStorage(), new FakeMembershipChecker(), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task ListVehicles_UtilizadorSemMembership_LancaMcpException()
    {
        await using var db = NewDb();

        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.ListVehicles(
            Guid.NewGuid(), db, new FakeObjectStorage(), new FakeMembershipChecker(isMember: false), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task ListVehicles_SemUtilizadorAutenticado_LancaMcpException()
    {
        await using var db = NewDb();

        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.ListVehicles(
            Guid.NewGuid(), db, new FakeObjectStorage(), new FakeMembershipChecker(), NewHttpContextAccessor(), CancellationToken.None));
    }

    [Fact]
    public async Task GetVehicle_DeOutroHousehold_LancaMcpException()
    {
        await using var db = NewDb();
        var vehicle = NewVehicle(Guid.NewGuid());
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.GetVehicle(
            Guid.NewGuid(), vehicle.Id, db, new FakeObjectStorage(), new FakeMembershipChecker(), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task CreateVehicle_Persiste()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var householdId = Guid.NewGuid();

        var result = await AutoMcpTools.CreateVehicle(
            householdId, VehicleCategory.Ligeiros, "Toyota", "Corolla", "AA-00-BB", "VIN123", "Branco", "Pedro",
            10_000, new DateOnly(2020, 1, 1), null, "Fidelidade", "AP-12345",
            new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1), 350.00m, null,
            db, events, new FakeObjectStorage(), new FakeMembershipChecker(), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal("Toyota", result.Brand);
        Assert.Equal(1, await db.Vehicles.CountAsync());
        Assert.Single(events.Published);
    }

    [Fact]
    public async Task UpdateVehicle_DeOutroHousehold_LancaMcpException()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var vehicle = NewVehicle(Guid.NewGuid());
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.UpdateVehicle(
            Guid.NewGuid(), vehicle.Id, VehicleCategory.Ligeiros, "Toyota", "Corolla", "AA-00-BB", "VIN123",
            "Azul", null, 10_000, new DateOnly(2020, 1, 1), null, null, null, null, null, null, null,
            db, events, new FakeObjectStorage(), new FakeMembershipChecker(), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task ArchiveVehicle_MudaEstado()
    {
        await using var db = NewDb();
        var events = new FakeEventPublisher();
        var householdId = Guid.NewGuid();
        var vehicle = NewVehicle(householdId);
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var result = await AutoMcpTools.ArchiveVehicle(
            householdId, vehicle.Id, VehicleStatus.Vendido, db, events, new FakeObjectStorage(), new FakeMembershipChecker(),
            NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(VehicleStatus.Vendido, result.Status);
    }

    [Fact]
    public async Task CreateMaintenance_VeiculoDeOutroHousehold_LancaMcpException()
    {
        await using var db = NewDb();
        var vehicle = NewVehicle(Guid.NewGuid());
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.CreateMaintenance(
            Guid.NewGuid(), vehicle.Id, new DateOnly(2026, 1, 1), 15_000, "Stand XPTO", "Revisão", MaintenanceType.Preventiva,
            120m, null, null, null, db, new FakeMembershipChecker(), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task CreateMaintenance_Persiste()
    {
        await using var db = NewDb();
        var householdId = Guid.NewGuid();
        var vehicle = NewVehicle(householdId);
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var result = await AutoMcpTools.CreateMaintenance(
            householdId, vehicle.Id, new DateOnly(2026, 1, 1), 15_000, "Stand XPTO", "Revisão", MaintenanceType.Preventiva,
            120m, null, null, null, db, new FakeMembershipChecker(), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal("Stand XPTO", result.Workshop);
        Assert.Equal(1, await db.Maintenances.CountAsync());
    }

    [Fact]
    public async Task DeleteDocument_Inexistente_LancaMcpException()
    {
        await using var db = NewDb();

        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.DeleteDocument(
            Guid.NewGuid(), Guid.NewGuid(), db, new FakeMembershipChecker(), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None));
    }
}
