namespace Zelo.Modules.Auto.Infrastructure;

internal sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Endpoint { get; set; } = "http://localhost:3900";
    public string Region { get; set; } = "garage";
    public string Bucket { get; set; } = "zelo-documents";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
}
