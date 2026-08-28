namespace Zelo.Modules.Identity.Infrastructure;

internal sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 1025;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; }
    public string FromAddress { get; set; } = "no-reply@zelo.local";
    public string FromName { get; set; } = "Zelo";
}
