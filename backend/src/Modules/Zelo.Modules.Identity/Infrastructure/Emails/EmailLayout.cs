using System.Net;

namespace Zelo.Modules.Identity.Infrastructure.Emails;

/// Substitui {{Placeholder}} pelos valores dados. Os campos que o
/// chamador passa como HTML/URL prontos (bodyHtml, buttonUrl) escapam a
/// codificacao - tudo o resto e tratado como texto simples.
///
/// buttonUrl NAO e codificado aqui: os dois chamadores (SmtpEmailSender,
/// para o link de confirmacao e o de reset de password) recebem-no do
/// UserManager/MapIdentityApi do ASP.NET Core Identity ja codificado
/// (HtmlEncoder.Default.Encode) - codificar outra vez transformava
/// "&" em "&amp;amp;", partindo o parsing da query string no clique
/// (o parametro "code" deixava de existir, so "amp;code").
internal static class EmailLayout
{
    public static string Render(string preheader, string heading, string bodyHtml, string buttonText, string buttonUrl)
    {
        var template = EmailTemplateLoader.Load("layout-button.html");

        return template
            .Replace("{{Preheader}}", WebUtility.HtmlEncode(preheader))
            .Replace("{{Heading}}", WebUtility.HtmlEncode(heading))
            .Replace("{{Body}}", bodyHtml)
            .Replace("{{ButtonText}}", WebUtility.HtmlEncode(buttonText))
            .Replace("{{ButtonUrlText}}", buttonUrl)
            .Replace("{{ButtonUrl}}", buttonUrl);
    }

    public static string RenderCode(string preheader, string heading, string bodyHtml, string code)
    {
        var template = EmailTemplateLoader.Load("layout-code.html");

        return template
            .Replace("{{Preheader}}", WebUtility.HtmlEncode(preheader))
            .Replace("{{Heading}}", WebUtility.HtmlEncode(heading))
            .Replace("{{Body}}", bodyHtml)
            .Replace("{{Code}}", WebUtility.HtmlEncode(code));
    }
}
