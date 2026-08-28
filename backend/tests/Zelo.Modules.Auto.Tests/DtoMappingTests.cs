using Xunit;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Endpoints;

namespace Zelo.Modules.Auto.Tests;

public class DtoMappingTests
{
    [Fact]
    public void VehicleResponse_From_MapsAllFields()
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            HouseholdId = Guid.NewGuid(),
            Category = VehicleCategory.Motociclos,
            Brand = "Yamaha",
            Model = "Tenere",
            Plate = "BA-07-NT",
            Vin = "JYARM09E4LA004178",
            Status = VehicleStatus.Ativo,
            Driver = "Pedro",
            Odometer = 24780,
            Registered = new DateOnly(2021, 6, 15),
            NextInspection = new DateOnly(2027, 6, 15),
            Insurer = "Fidelidade",
        };

        var response = VehicleResponse.From(vehicle);

        Assert.Equal(vehicle.Id, response.Id);
        Assert.Equal(VehicleCategory.Motociclos, response.Category);
        Assert.Equal("Yamaha", response.Brand);
        Assert.Equal(24780, response.Odometer);
        Assert.Equal(new DateOnly(2021, 6, 15), response.Registered);
        Assert.Equal("Fidelidade", response.Insurer);
    }

    [Fact]
    public void MaintenanceResponse_From_MapsItems()
    {
        var maintenance = new Maintenance
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2026, 9, 12),
            Odometer = 24650,
            Workshop = "Auto Serviço Silva",
            Description = "Troca de óleo",
            Type = MaintenanceType.Preventiva,
            Cost = 85.00m,
        };
        maintenance.Items.Add(new MaintenanceItem
        {
            Id = Guid.NewGuid(),
            MaintenanceId = maintenance.Id,
            Description = "Óleo 10W-40",
            Price = 35.00m,
            SerialNumber = "FO-2281",
        });

        var response = MaintenanceResponse.From(maintenance);

        Assert.Equal(maintenance.Id, response.Id);
        Assert.Equal(85.00m, response.Cost);
        Assert.Equal(MaintenanceType.Preventiva, response.Type);
        Assert.Single(response.Items);
        Assert.Equal("Óleo 10W-40", response.Items[0].Description);
        Assert.Equal("FO-2281", response.Items[0].SerialNumber);
    }

    [Fact]
    public void MaintenanceResponse_From_EmptyItems_MapsToEmptyList()
    {
        var maintenance = new Maintenance
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Workshop = "Oficina",
            Description = "Inspeção",
            Type = MaintenanceType.Inspecao,
        };

        var response = MaintenanceResponse.From(maintenance);

        Assert.Empty(response.Items);
    }

    [Fact]
    public void DocumentResponse_From_MapsAllFields()
    {
        var document = new VehicleDocument
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Name = "Apólice de seguro",
            Category = DocumentCategory.Seguro,
            Type = DocumentType.Pdf,
            Date = new DateOnly(2026, 1, 1),
            SizeBytes = 128_000,
            ObjectKey = "vehicles/x/y.pdf",
        };

        var response = DocumentResponse.From(document);

        Assert.Equal(document.Id, response.Id);
        Assert.Equal("Apólice de seguro", response.Name);
        Assert.Equal(DocumentCategory.Seguro, response.Category);
        Assert.Equal(DocumentType.Pdf, response.Type);
        Assert.Equal(128_000, response.SizeBytes);
    }
}
