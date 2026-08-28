using Xunit;
using Zelo.Modules.Identity.Infrastructure.Emails;

namespace Zelo.Modules.Identity.Tests;

public class EmailLayoutTests
{
    [Fact]
    public void Render_SubstituiTodosOsPlaceholders()
    {
        var html = EmailLayout.Render(
            preheader: "Confirme o seu email",
            heading: "Bem-vindo",
            bodyHtml: "<p>Clique no botao abaixo.</p>",
            buttonText: "Confirmar",
            buttonUrl: "https://zelo.pt/confirm?token=abc");

        Assert.Contains("Confirme o seu email", html);
        Assert.Contains("Bem-vindo", html);
        Assert.Contains("<p>Clique no botao abaixo.</p>", html);
        Assert.Contains("Confirmar", html);
        Assert.Contains("https://zelo.pt/confirm?token=abc", html);
        Assert.DoesNotContain("{{", html);
    }

    [Fact]
    public void Render_CodificaCamposDeTextoMasNaoOBodyHtml()
    {
        var html = EmailLayout.Render(
            preheader: "<script>alert(1)</script>",
            heading: "Titulo",
            bodyHtml: "<p>seguro</p>",
            buttonText: "Ok",
            buttonUrl: "https://zelo.pt");

        Assert.DoesNotContain("<script>alert(1)</script>", html);
        Assert.Contains("&lt;script&gt;", html);
        Assert.Contains("<p>seguro</p>", html);
    }

    [Fact]
    public void RenderCode_SubstituiPlaceholdersIncluindoOCodigo()
    {
        var html = EmailLayout.RenderCode(
            preheader: "O seu codigo",
            heading: "Repor palavra-passe",
            bodyHtml: "<p>Use o codigo abaixo.</p>",
            code: "482913");

        Assert.Contains("O seu codigo", html);
        Assert.Contains("Repor palavra-passe", html);
        Assert.Contains("<p>Use o codigo abaixo.</p>", html);
        Assert.Contains("482913", html);
        Assert.DoesNotContain("{{", html);
    }

    [Fact]
    public void RenderCode_CodificaOCodigoComoTexto()
    {
        var html = EmailLayout.RenderCode("pre", "head", "<p>body</p>", "<b>123</b>");

        Assert.DoesNotContain("<b>123</b>", html);
        Assert.Contains("&lt;b&gt;123&lt;/b&gt;", html);
    }
}
