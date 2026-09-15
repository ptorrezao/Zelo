using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto.Endpoints;

internal sealed record VehicleUpsertRequest(
    VehicleCategory Category,
    string Brand,
    string Model,
    string Plate,
    string Vin,
    string? Color,
    string? Driver,
    int Odometer,
    DateOnly Registered,
    DateOnly? NextInspection,
    string? Insurer,
    string? InsurancePolicyNumber,
    DateOnly? InsurancePeriodStart,
    DateOnly? InsurancePeriodEnd,
    decimal? InsurancePremium,
    DateOnly? IucDueDate);

internal sealed record VehicleResponse(
    Guid Id,
    VehicleCategory Category,
    string Brand,
    string Model,
    string Plate,
    string Vin,
    string? Color,
    VehicleStatus Status,
    string? Driver,
    int Odometer,
    DateOnly Registered,
    DateOnly? NextInspection,
    string? Insurer,
    string? InsurancePolicyNumber,
    DateOnly? InsurancePeriodStart,
    DateOnly? InsurancePeriodEnd,
    decimal? InsurancePremium,
    DateOnly? IucDueDate,
    string? PhotoUrl)
{
    // storage so serve para resolver a URL de leitura pre-assinada da
    // foto (ver IObjectStorage.CreateReadUrl) - PhotoObjectKey em si
    // nunca sai da Api, so a URL temporaria.
    public static VehicleResponse From(Vehicle v, IObjectStorage storage) => new(
        v.Id, v.Category, v.Brand, v.Model, v.Plate, v.Vin, v.Color, v.Status, v.Driver,
        v.Odometer, v.Registered, v.NextInspection, v.Insurer, v.InsurancePolicyNumber,
        v.InsurancePeriodStart, v.InsurancePeriodEnd, v.InsurancePremium, v.IucDueDate,
        v.PhotoObjectKey is { } key ? storage.CreateReadUrl(key, TimeSpan.FromMinutes(15)).ToString() : null);
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
    long SizeBytes,
    string DownloadUrl)
{
    // Mesmo padrao de VehicleResponse.PhotoUrl - o ObjectKey nunca sai da
    // Api, so uma URL de leitura pre-assinada e temporaria.
    public static DocumentResponse From(VehicleDocument d, IObjectStorage storage) => new(
        d.Id, d.Name, d.Category, d.Type, d.Date, d.SizeBytes,
        storage.CreateReadUrl(d.ObjectKey, TimeSpan.FromMinutes(15)).ToString());
}

internal sealed record VehicleStatsResponse(
    int KmsLastMonth,
    decimal MaintenanceCostLastMonth,
    int MaintenanceCountLastMonth);

internal sealed record ImportConnectRequest(string BaseUrl, string Email, string Password, Guid? RemoteHouseholdId);

internal enum ImportPreviewStatus { ChooseHousehold, VehiclesReady }

internal sealed record ImportHouseholdOption(Guid Id, string Name);

internal sealed record ImportCandidateVehicle(
    VehicleCategory Category,
    string Brand,
    string Model,
    string Plate,
    string Vin,
    string? Color,
    string? Driver,
    int Odometer,
    DateOnly Registered,
    DateOnly? NextInspection,
    string? Insurer,
    string? InsurancePolicyNumber,
    DateOnly? InsurancePeriodStart,
    DateOnly? InsurancePeriodEnd,
    decimal? InsurancePremium,
    DateOnly? IucDueDate);

internal sealed record ImportPreviewItem(ImportCandidateVehicle Vehicle, bool AlreadyExists);

/// Households so vem preenchido quando Status = ChooseHousehold (o
/// utilizador de origem tem mais que um, o frontend tem de escolher e
/// chamar o preview outra vez com RemoteHouseholdId). Vehicles so vem
/// preenchido quando Status = VehiclesReady.
internal sealed record ImportPreviewResponse(
    ImportPreviewStatus Status,
    IReadOnlyList<ImportHouseholdOption> Households,
    IReadOnlyList<ImportPreviewItem> Vehicles);

internal sealed record ImportConfirmRequest(IReadOnlyList<ImportCandidateVehicle> Vehicles);

internal sealed record ImportResultItem(string Plate, bool Imported, string? SkipReason);

internal sealed record ImportConfirmResponse(int ImportedCount, int SkippedCount, IReadOnlyList<ImportResultItem> Items);
