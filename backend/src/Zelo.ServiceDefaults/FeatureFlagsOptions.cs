namespace Zelo.ServiceDefaults;

public sealed class FeatureFlagsOptions
{
    public const string SectionName = "FeatureFlags";

    public string? Url { get; set; }
    public string? ApiToken { get; set; }
    public string Environment { get; set; } = "development";
}
