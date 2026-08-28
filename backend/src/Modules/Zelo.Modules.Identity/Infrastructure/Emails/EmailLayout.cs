using System.Net;

namespace Zelo.Modules.Identity.Infrastructure.Emails;

/// Substitui {{Placeholder}} pelos valores dados. So os campos que o
/// chamador passa como HTML pronto (bodyHtml) escapam a codificacao -
/// tudo o resto e tratado como texto simples.
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
            .Replace("{{ButtonUrlText}}", WebUtility.HtmlEncode(buttonUrl))
            .Replace("{{ButtonUrl}}", WebUtility.HtmlEncode(buttonUrl));
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
