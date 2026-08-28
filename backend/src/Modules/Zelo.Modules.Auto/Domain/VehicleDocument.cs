namespace Zelo.Modules.Auto.Domain;

internal sealed class VehicleDocument
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public required string Name { get; set; }
    public DocumentCategory Category { get; set; }
    public DocumentType Type { get; set; }
    public DateOnly Date { get; set; }
    public long SizeBytes { get; set; }
    public required string ObjectKey { get; set; }
    public DateTimeOffset UploadedAt { get; init; }
}
