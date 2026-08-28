using System.Diagnostics;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Zelo.Modules.Identity.Domain;
using Zelo.Modules.Identity.Infrastructure.Emails;

namespace Zelo.Modules.Identity.Infrastructure;

/// Em dev aponta para o Mailhog (nunca envia de verdade, so captura).
/// Em producao, EmailOptions aponta para um SMTP real.
internal sealed class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender<ZeloUser>
{
    /// System.Net.Mail nao tem instrumentacao OpenTelemetry oficial - sem
    /// isto o envio de email fica invisivel dentro do span do pedido HTTP
    /// que o disparou (ex.: /api/auth/register).
    private static readonly ActivitySource ActivitySource = new("Zelo.Modules.Identity.Email");

    private readonly EmailOptions _options = options.Value;

    public Task SendConfirmationLinkAsync(ZeloUser user, string email, string confirmationLink)
    {
        var body = EmailLayout.Render(
            preheader: "Confirme o seu email para ativar a conta Zelo.",
            heading: "Confirme a sua conta",
            bodyHtml: "Falta pouco. Clique no botão abaixo para confirmar o seu email e ativar a sua conta Zelo.",
            buttonText: "Confirmar conta",
            buttonUrl: confirmationLink);

        return SendAsync("confirmation-link", email, "Confirme a sua conta Zelo", body);
    }

    public Task SendPasswordResetLinkAsync(ZeloUser user, string email, string resetLink)
    {
        var body = EmailLayout.Render(
            preheader: "Pediu para repor a palavra-passe da sua conta Zelo.",
            heading: "Repor palavra-passe",
            bodyHtml: "Recebemos um pedido para repor a palavra-passe da sua conta. Clique no botão abaixo para escolher uma nova. Este link expira em breve.",
            buttonText: "Repor palavra-passe",
            buttonUrl: resetLink);

        return SendAsync("password-reset-link", email, "Repor palavra-passe Zelo", body);
    }

    public Task SendPasswordResetCodeAsync(ZeloUser user, string email, string resetCode)
    {
        var body = EmailLayout.RenderCode(
            preheader: "O seu código para repor a palavra-passe Zelo.",
            heading: "Repor palavra-passe",
            bodyHtml: "Use o código abaixo para repor a palavra-passe da sua conta Zelo.",
            code: resetCode);

        return SendAsync("password-reset-code", email, "Código para repor palavra-passe Zelo", body);
    }

    private async Task SendAsync(string kind, string toEmail, string subject, string htmlBody)
    {
        using var activity = ActivitySource.StartActivity($"email.send.{kind}", ActivityKind.Producer);
        activity?.SetTag("email.kind", kind);
        activity?.SetTag("email.to", toEmail);

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort);
        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(toEmail);

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
