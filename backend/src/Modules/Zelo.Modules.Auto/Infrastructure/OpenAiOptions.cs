namespace Zelo.Modules.Auto.Infrastructure;

internal sealed class OpenAiOptions
{
    public const string SectionName = "OpenAi";

    public string ApiKey { get; set; } = "";
}
