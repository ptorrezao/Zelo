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
    private sealed class FakeMembershipChecker(bool isMember = true) : IHouseholdMembershipChecker
    {
        public Task<bool> IsMemberAsync(Guid userId, Guid householdId, CancellationToken ct = default) =>
            Task.FromResult(isMember);
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
    public async Task ListVehicles_UtilizadorMembro_DevolveVeiculosDoHousehold()
    {
        await using var db = NewDb();
        var householdId = Guid.NewGuid();
        db.Vehicles.Add(NewVehicle(householdId));
        db.Vehicles.Add(NewVehicle(Guid.NewGuid())); // outro household
        await db.SaveChangesAsync();

        var result = await AutoMcpTools.ListVehicles(
            householdId, db, new FakeMembershipChecker(), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task ListVehicles_UtilizadorSemMembership_LancaMcpException()
    {
        await using var db = NewDb();

        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.ListVehicles(
            Guid.NewGuid(), db, new FakeMembershipChecker(isMember: false), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task ListVehicles_SemUtilizadorAutenticado_LancaMcpException()
    {
        await using var db = NewDb();

        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.ListVehicles(
            Guid.NewGuid(), db, new FakeMembershipChecker(), NewHttpContextAccessor(), CancellationToken.None));
    }

    [Fact]
    public async Task GetVehicle_DeOutroHousehold_LancaMcpException()
    {
        await using var db = NewDb();
        var vehicle = NewVehicle(Guid.NewGuid());
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<McpException>(() => AutoMcpTools.GetVehicle(
            Guid.NewGuid(), vehicle.Id, db, new FakeMembershipChecker(), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None));
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
            db, events, new FakeMembershipChecker(), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None);

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
            db, events, new FakeMembershipChecker(), NewHttpContextAccessor(Guid.NewGuid()), CancellationToken.None));
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
            householdId, vehicle.Id, VehicleStatus.Vendido, db, events, new FakeMembershipChecker(),
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
