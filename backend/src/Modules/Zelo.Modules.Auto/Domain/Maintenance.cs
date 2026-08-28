namespace Zelo.Modules.Auto.Domain;

internal sealed class Maintenance
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public DateOnly Date { get; set; }
    public int Odometer { get; set; }
    public required string Workshop { get; set; }
    public required string Description { get; set; }
    public MaintenanceType Type { get; set; }
    public decimal Cost { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public string? InvoiceObjectKey { get; set; }

    public List<MaintenanceItem> Items { get; init; } = [];
}

internal sealed class MaintenanceItem
{
    public Guid Id { get; init; }
    public Guid MaintenanceId { get; init; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public string? SerialNumber { get; set; }
}
