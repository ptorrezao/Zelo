namespace Zelo.Modules.Core.Infrastructure;

/// Config SMTP propria do Core: EmailOptions do Identity e internal a
/// esse modulo (module boundary impede reutilizar). Mesmos defaults
/// (aponta para o Mailhog local em dev).
internal sealed class NotificationEmailOptions
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
