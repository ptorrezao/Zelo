using System.Net;

namespace Zelo.Modules.Core.Infrastructure.Emails;

/// Substitui {{Placeholder}} pelos valores dados. bodyHtml e passado
/// pronto (o chamador ja construiu o HTML do corpo) - tudo o resto e
/// texto simples, codificado aqui.
internal static class NotificationEmailLayout
{
    public static string Render(string preheader, string heading, string bodyHtml)
    {
        var template = NotificationEmailTemplateLoader.Load("reminder.html");

        return template
            .Replace("{{Preheader}}", WebUtility.HtmlEncode(preheader))
            .Replace("{{Heading}}", WebUtility.HtmlEncode(heading))
            .Replace("{{Body}}", bodyHtml);
    }
}
