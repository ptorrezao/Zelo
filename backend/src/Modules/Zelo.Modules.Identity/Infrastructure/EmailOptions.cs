namespace Zelo.Modules.Identity.Infrastructure;

internal sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 1025;
    public string FromAddress { get; set; } = "no-reply@zelo.local";
    public string FromName { get; set; } = "Zelo";
}
