namespace Zelo.Modules.Auto.Domain;

internal sealed class Vehicle
{
    public Guid Id { get; init; }
    public Guid HouseholdId { get; set; }
    public required VehicleCategory Category { get; set; }
    public required string Brand { get; set; }
    public required string Model { get; set; }
    public required string Plate { get; set; }
    public required string Vin { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.Ativo;
    public string? Color { get; set; }
    public string? Driver { get; set; }
    public int Odometer { get; set; }
    public DateOnly Registered { get; set; }
    public DateOnly? NextInspection { get; set; }
    public string? Insurer { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public DateOnly? InsurancePeriodStart { get; set; }
    public DateOnly? InsurancePeriodEnd { get; set; }
    public decimal? InsurancePremium { get; set; }
    public DateOnly? IucDueDate { get; set; }
    public DateTimeOffset CreatedAt { get; init; }

    /// Id da Obligation no Core criada/atualizada a partir de NextInspection.
    /// Nulo se NextInspection nunca foi definido. Atribuido pelo Auto (ver
    /// ADR-002 / module-contract.md), reutilizado em updates para o Core
    /// saber que e a mesma obrigacao a reagendar, nao uma nova.
    public Guid? InspectionObligationId { get; set; }

    /// Id da Obligation no Core criada/atualizada a partir de
    /// InsurancePeriodEnd. Mesmo padrao que InspectionObligationId.
    public Guid? InsuranceObligationId { get; set; }

    /// Object key no Garage da foto gerada automaticamente para este
    /// veiculo (ver VehiclePhotoHandler). Nulo ate a geracao acontecer ou
    /// se tiver falhado - nesse caso o frontend mostra o placeholder de
    /// sempre, sem retry automatico nesta versao.
    public string? PhotoObjectKey { get; set; }

    public List<Maintenance> Maintenances { get; init; } = [];
    public List<VehicleDocument> Documents { get; init; } = [];
}
