using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Zelo.Modules.Core.Infrastructure.Emails;

namespace Zelo.Modules.Core.Infrastructure;

/// Em dev aponta para o Mailhog (nunca envia de verdade, so captura).
/// Em producao, NotificationEmailOptions aponta para um SMTP real.
internal sealed class SmtpNotificationSender(IOptions<NotificationEmailOptions> options) : INotificationEmailSender
{
    private static readonly ActivitySource ActivitySource = new("Zelo.Modules.Core.Email");

    private readonly NotificationEmailOptions _options = options.Value;

    public async Task SendObligationReminderAsync(string toEmail, string obligationTitle, DateOnly dueOn, int daysUntilDue, CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity("email.send.obligation-reminder", ActivityKind.Producer);
        activity?.SetTag("email.to", toEmail);

        var dueOnText = dueOn.ToString("d MMMM yyyy", new CultureInfo("pt-PT"));
        var bodyHtml = daysUntilDue > 0
            ? $"A obrigação <strong>{WebUtility.HtmlEncode(obligationTitle)}</strong> vence a {dueOnText} (daqui a {daysUntilDue} dia{(daysUntilDue == 1 ? "" : "s")})."
            : $"A obrigação <strong>{WebUtility.HtmlEncode(obligationTitle)}</strong> vence hoje ({dueOnText}).";

        var body = NotificationEmailLayout.Render(
            preheader: $"Lembrete: {obligationTitle}",
            heading: "Lembrete de obrigação",
            bodyHtml: bodyHtml);

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
        };
        if (!string.IsNullOrEmpty(_options.Username))
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = $"Lembrete: {obligationTitle}",
            Body = body,
            IsBodyHtml = true,
        };
        message.To.Add(toEmail);

        try
        {
            await client.SendMailAsync(message, ct);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
