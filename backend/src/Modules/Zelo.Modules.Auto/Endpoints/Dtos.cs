using Zelo.Modules.Auto.Domain;

namespace Zelo.Modules.Auto.Endpoints;

internal sealed record VehicleUpsertRequest(
    VehicleCategory Category,
    string Brand,
    string Model,
    string Plate,
    string Vin,
    string? Driver,
    int Odometer,
    DateOnly Registered,
    DateOnly? NextInspection,
    string? Insurer,
    DateOnly? InsuranceRenewal,
    DateOnly? IucDueDate);

internal sealed record VehicleResponse(
    Guid Id,
    VehicleCategory Category,
    string Brand,
    string Model,
    string Plate,
    string Vin,
    VehicleStatus Status,
    string? Driver,
    int Odometer,
    DateOnly Registered,
    DateOnly? NextInspection,
    string? Insurer,
    DateOnly? InsuranceRenewal,
    DateOnly? IucDueDate)
{
    public static VehicleResponse From(Vehicle v) => new(
        v.Id, v.Category, v.Brand, v.Model, v.Plate, v.Vin, v.Status, v.Driver,
        v.Odometer, v.Registered, v.NextInspection, v.Insurer, v.InsuranceRenewal, v.IucDueDate);
}

internal sealed record MaintenanceItemRequest(string Description, decimal Price, string? SerialNumber);

internal sealed record MaintenanceUpsertRequest(
    DateOnly Date,
    int Odometer,
    string Workshop,
    string Description,
    MaintenanceType Type,
    decimal Cost,
    string? InvoiceNumber,
    DateOnly? InvoiceDate,
    IReadOnlyList<MaintenanceItemRequest>? Items);

internal sealed record MaintenanceItemResponse(Guid Id, string Description, decimal Price, string? SerialNumber)
{
    public static MaintenanceItemResponse From(MaintenanceItem i) => new(i.Id, i.Description, i.Price, i.SerialNumber);
}

internal sealed record MaintenanceResponse(
    Guid Id,
    Guid VehicleId,
    DateOnly Date,
    int Odometer,
    string Workshop,
    string Description,
    MaintenanceType Type,
    decimal Cost,
    string? InvoiceNumber,
    DateOnly? InvoiceDate,
    IReadOnlyList<MaintenanceItemResponse> Items)
{
    public static MaintenanceResponse From(Maintenance m) => new(
        m.Id, m.VehicleId, m.Date, m.Odometer, m.Workshop, m.Description, m.Type, m.Cost,
        m.InvoiceNumber, m.InvoiceDate, [.. m.Items.Select(MaintenanceItemResponse.From)]);
}

internal sealed record UploadUrlRequest(string FileName, string ContentType);

internal sealed record UploadUrlResponse(string ObjectKey, Uri UploadUrl, DateTimeOffset ExpiresAt);

internal sealed record DocumentCreateRequest(
    string ObjectKey,
    string Name,
    DocumentCategory Category,
    DocumentType Type,
    DateOnly Date,
    long SizeBytes);

internal sealed record DocumentResponse(
    Guid Id,
    string Name,
    DocumentCategory Category,
    DocumentType Type,
    DateOnly Date,
    long SizeBytes)
{
    public static DocumentResponse From(VehicleDocument d) => new(d.Id, d.Name, d.Category, d.Type, d.Date, d.SizeBytes);
}

internal sealed record VehicleStatsResponse(
    int KmsLastMonth,
    decimal MaintenanceCostLastMonth,
    int MaintenanceCountLastMonth);
