namespace Zelo.Modules.Auto.Domain;

internal sealed class Vehicle
{
    public Guid Id { get; init; }
    public Guid HouseholdId { get; init; }
    public required VehicleCategory Category { get; set; }
    public required string Brand { get; set; }
    public required string Model { get; set; }
    public required string Plate { get; set; }
    public required string Vin { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.Ativo;
    public string? Driver { get; set; }
    public int Odometer { get; set; }
    public DateOnly Registered { get; set; }
    public DateOnly? NextInspection { get; set; }
    public string? Insurer { get; set; }
    public DateOnly? InsuranceRenewal { get; set; }
    public DateOnly? IucDueDate { get; set; }
    public DateTimeOffset CreatedAt { get; init; }

    /// Id da Obligation no Core criada/atualizada a partir de NextInspection.
    /// Nulo se NextInspection nunca foi definido. Atribuido pelo Auto (ver
    /// ADR-002 / module-contract.md), reutilizado em updates para o Core
    /// saber que e a mesma obrigacao a reagendar, nao uma nova.
    public Guid? InspectionObligationId { get; set; }

    public List<Maintenance> Maintenances { get; init; } = [];
    public List<VehicleDocument> Documents { get; init; } = [];
}
